/*
 * Copyright © 2013 Ben Bader
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

﻿using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Abra.Fody.Generators
{
    public class ProviderBindingGenerator : Generator
    {
        private readonly TypeReference providedType;
        private readonly string providerKey;

        private MethodReference ctor;

        public ProviderBindingGenerator(ModuleDefinition moduleDefinition, string providerKey, TypeReference providedType)
            : base(moduleDefinition)
        {
            this.providerKey = Conditions.CheckNotNull(providerKey, "providerKey");
            this.providedType = Conditions.CheckNotNull(providedType, "providedType");
        }

        public override void Validate(IWeaver weaver)
        {
        }

        public override TypeDefinition Generate(IWeaver weaver)
        {
            var t = new TypeDefinition(
                providedType.Namespace,
                providedType.Name + Internal.Plugins.Codegen.CodegenPlugin.IProviderSuffix,
                TypeAttributes.Public,
                References.Binding);

            var providerOfT = References.IProviderOfT.MakeGenericInstanceType(providedType);

            t.Interfaces.Add(providerOfT);
            t.CustomAttributes.Add(new CustomAttribute(References.CompilerGeneratedAttribute));

            var providerOfT_get = ModuleDefinition.Import(providerOfT.Resolve()
                                                                     .Methods
                                                                     .First(m => m.Name == "Get"))
                                                  .MakeHostInstanceGeneric(providedType);

            var providerKeyField = new FieldDefinition("providerKey", FieldAttributes.Private, ModuleDefinition.TypeSystem.String);
            var mustBeInjectableField = new FieldDefinition("mustBeInjectable", FieldAttributes.Private, ModuleDefinition.TypeSystem.Boolean);
            var delegateBindingField = new FieldDefinition("delegateBinding", FieldAttributes.Private, References.Binding);

            t.Fields.Add(providerKeyField);
            t.Fields.Add(mustBeInjectableField);
            t.Fields.Add(delegateBindingField);

            EmitCtor(t, providerKeyField, mustBeInjectableField);
            EmitResolve(t, mustBeInjectableField, providerKeyField, delegateBindingField);
            EmitGet(t, providerOfT_get, delegateBindingField);

            return t;
        }

        public override KeyedCtor GetKeyedCtor()
        {
            Conditions.CheckNotNull(ctor);
            return new KeyedCtor(providerKey, ctor);
        }

        private void EmitCtor(TypeDefinition providerBinding, FieldDefinition providerKeyField, FieldDefinition mustBeInjectableField)
        {
            var ctor = new MethodDefinition(
                ".ctor",
                MethodAttributes.Public | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                ModuleDefinition.TypeSystem.Void);

            ctor.Parameters.Add(new ParameterDefinition("key", ParameterAttributes.None, ModuleDefinition.TypeSystem.String));
            ctor.Parameters.Add(new ParameterDefinition("requiredBy", ParameterAttributes.None, ModuleDefinition.TypeSystem.Object));
            ctor.Parameters.Add(new ParameterDefinition("mustBeInjectable", ParameterAttributes.None, ModuleDefinition.TypeSystem.Boolean));
            ctor.Parameters.Add(new ParameterDefinition("providerKey", ParameterAttributes.None, ModuleDefinition.TypeSystem.String));

            var il = ctor.Body.GetILProcessor();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);  // key
            il.Emit(OpCodes.Ldnull);   // memberKey
            il.EmitBoolean(false);     // mustBeInjectable
            il.Emit(OpCodes.Ldarg_2);  // requiredBy
            il.Emit(OpCodes.Call, References.Binding_Ctor);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_3);
            il.Emit(OpCodes.Stfld, mustBeInjectableField);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_S, ctor.Parameters.Last());
            il.Emit(OpCodes.Stfld, providerKeyField);

            il.Emit(OpCodes.Ret);

            providerBinding.Methods.Add(ctor);
            this.ctor = ctor;
        }

        private void EmitResolve(TypeDefinition providerBinding, FieldDefinition mustBeInjectableField,
                                 FieldDefinition providerKeyField, FieldDefinition delegateBindingField)
        {
            var resolve = new MethodDefinition(
                "Resolve",
                MethodAttributes.Public | MethodAttributes.Virtual,
                ModuleDefinition.TypeSystem.Void);

            resolve.Parameters.Add(new ParameterDefinition(References.Resolver));
            resolve.Overrides.Add(References.Binding_Resolve);

            var il = resolve.Body.GetILProcessor();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, providerKeyField);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, References.Binding_RequiredByGetter);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, mustBeInjectableField);
            il.Emit(OpCodes.Callvirt, References.Resolver_RequestBinding);
            il.Emit(OpCodes.Stfld, delegateBindingField);
            il.Emit(OpCodes.Ret);

            providerBinding.Methods.Add(resolve);
        }

        private void EmitGet(TypeDefinition providerBinding, MethodReference providerOfT_get, FieldDefinition delegateBindingField)
        {
            var get = new MethodDefinition(
                "Get",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                ModuleDefinition.TypeSystem.Object);

            var getExplicit = new MethodDefinition(
                "Get",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                providedType);

            get.Overrides.Add(References.Binding_Get);
            getExplicit.Overrides.Add(providerOfT_get);
            
            // First we emit a helper method to serve as the body of a Func<T>,
            // because lambdas don't exist in IL
            var il = getExplicit.Body.GetILProcessor();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, delegateBindingField);
            il.Emit(OpCodes.Callvirt, References.Binding_Get);
            il.Cast(providedType);
            il.Emit(OpCodes.Ret);
            providerBinding.Methods.Add(getExplicit);

            // Then we actually emit Get(), which is just `return this;`
            il = get.Body.GetILProcessor();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ret);
            providerBinding.Methods.Add(get);
        }
    }
}
