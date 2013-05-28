﻿using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Stiletto.Fody
{
    public class StilettoReferences
    {
        public TypeDefinition Binding { get; private set; }
        public MethodDefinition Binding_Ctor { get; private set; }
        public MethodDefinition Binding_GetDependencies { get; private set; }
        public MethodDefinition Binding_Resolve { get; private set; }
        public MethodDefinition Binding_Get { get; private set; }
        public MethodDefinition Binding_InjectProperties { get; private set; }
        public MethodDefinition Binding_RequiredBy_Getter { get; private set; }
        public MethodDefinition Binding_IsLibrary_Setter { get; private set; }

        public TypeDefinition ProviderMethodBindingBase { get; private set; }
        public MethodDefinition ProviderMethodBindingBase_Ctor { get; private set; }

        public TypeDefinition RuntimeModule { get; private set; }
        public MethodDefinition RuntimeModule_Ctor { get; private set; }
        public MethodDefinition RuntimeModule_Module_Getter { get; private set; }

        public TypeDefinition Container { get; private set; }
        public MethodDefinition Container_Create { get; private set; }
        public MethodDefinition Container_CreateWithPlugins { get; private set; }

        public TypeDefinition IPlugin { get; private set; }
        public MethodDefinition IPlugin_GetInjectBinding { get; private set; }
        public MethodDefinition IPlugin_GetLazyInjectBinding { get; private set; }
        public MethodDefinition IPlugin_GetIProviderInjectBinding { get; private set; }
        public MethodDefinition IPlugin_GetRuntimeModue { get; private set; }

        public TypeDefinition Resolver { get; private set; }
        public MethodDefinition Resolver_RequestBinding { get; private set; }

        public TypeDefinition IProviderOfT { get; private set; }
        public MethodDefinition IProviderOfT_Get { get; private set; }

        public TypeDefinition InjectAttribute { get; private set; }
        public TypeDefinition ModuleAttribute { get; private set; }
        public TypeDefinition ProvidesAttribute { get; private set; }
        public TypeDefinition NamedAttribute { get; private set; }
        public TypeDefinition SingletonAttribute { get; private set; }

        public TypeDefinition ProcessedAssemblyAttribute { get; private set; }
        public MethodDefinition ProcessedAssemblyAttribute_Ctor { get; private set; }

        private StilettoReferences()
        {
            
        }

        public static StilettoReferences Create(IAssemblyResolver assemblyResolver)
        {
            var stiletto = assemblyResolver.Resolve("Stiletto").MainModule;
            var types = stiletto
                .GetAllTypes()
                .Where(t => t.IsPublic)
                .ToDictionary(t => t.FullName, t => t, StringComparer.Ordinal);

            var tBinding = types["Stiletto.Internal.Binding"];
            var tBinding_ctor = tBinding.GetMethod(".ctor");
            var tBinding_GetDependencies = tBinding.GetMethod("GetDependencies");
            var tBinding_Resolve = tBinding.GetMethod("Resolve");
            var tBinding_Get = tBinding.GetMethod("Get");
            var tBinding_InjectProperties = tBinding.GetMethod("InjectProperties");
            var tBinding_RequiredBy_Getter = tBinding.GetProperty("RequiredBy").GetMethod;
            var tBinding_IsLibrary_Setter = tBinding.GetProperty("IsLibrary").SetMethod;

            var tProviderMethodBindingBase = types["Stiletto.Internal.ProviderMethodBindingBase"];
            var tProviderMethodBingingBase_ctor = tProviderMethodBindingBase.GetMethod(".ctor");

            var tRuntimeModule = types["Stiletto.Internal.RuntimeModule"];
            var tRuntimeModule_ctor = tRuntimeModule.GetMethod(".ctor");
            var tRuntimeModule_module_getter = tRuntimeModule.GetProperty("Module").GetMethod;

            var tContainer = types["Stiletto.Container"];
            var tContainer_Create = tContainer.GetMethod("Create");
            var tContainer_CreateWithPlugins = tContainer.GetMethod("CreateWithPlugins");

            var tPlugin = types["Stiletto.Internal.IPlugin"];
            var tPlugin_GetInjectBinding = tPlugin.GetMethod("GetInjectBinding");
            var tPlugin_GetLazyInjectBinding = tPlugin.GetMethod("GetLazyInjectBinding");
            var tPlugin_GetProviderInjectBinding = tPlugin.GetMethod("GetIProviderInjectBinding");
            var tPlugin_GetRuntimeModule = tPlugin.GetMethod("GetRuntimeModule");

            var tResolver = types["Stiletto.Internal.Resolver"];
            var tResolver_RequestBinding = tResolver.GetMethod("RequestBinding");

            var tProviderOfT = types["Stiletto.IProvider`1"];
            var tProviderOfT_Get = tProviderOfT.GetMethod("Get");

            var tInjectAttribute = types["Stiletto.InjectAttribute"];
            var tModuleAttribute = types["Stiletto.ModuleAttribute"];
            var tProvidesAttribute = types["Stiletto.ProvidesAttribute"];
            var tNamedAttribute = types["Stiletto.NamedAttribute"];
            var tSingletonAttribute = types["Stiletto.SingletonAttribute"];

            var tProcessedAssemblyAttribute = types["Stiletto.Internal.Plugins.Codegen.ProcessedAssemblyAttribute"];
            var tProcessedAssemblyAttribute_Ctor = tProcessedAssemblyAttribute.GetDefaultConstructor();

            return new StilettoReferences
                       {
                           Binding = tBinding,
                           Binding_Ctor = tBinding_ctor,
                           Binding_GetDependencies = tBinding_GetDependencies,
                           Binding_Resolve = tBinding_Resolve,
                           Binding_Get = tBinding_Get,
                           Binding_InjectProperties = tBinding_InjectProperties,
                           Binding_RequiredBy_Getter = tBinding_RequiredBy_Getter,
                           Binding_IsLibrary_Setter = tBinding_IsLibrary_Setter,

                           ProviderMethodBindingBase = tProviderMethodBindingBase,
                           ProviderMethodBindingBase_Ctor = tProviderMethodBingingBase_ctor,

                           RuntimeModule = tRuntimeModule,
                           RuntimeModule_Ctor = tRuntimeModule_ctor,
                           RuntimeModule_Module_Getter = tRuntimeModule_module_getter,

                           Container = tContainer,
                           Container_Create = tContainer_Create,
                           Container_CreateWithPlugins = tContainer_CreateWithPlugins,

                           IPlugin = tPlugin,
                           IPlugin_GetInjectBinding = tPlugin_GetInjectBinding,
                           IPlugin_GetLazyInjectBinding = tPlugin_GetLazyInjectBinding,
                           IPlugin_GetIProviderInjectBinding = tPlugin_GetProviderInjectBinding,
                           IPlugin_GetRuntimeModue = tPlugin_GetRuntimeModule,

                           Resolver = tResolver,
                           Resolver_RequestBinding = tResolver_RequestBinding,

                           IProviderOfT = tProviderOfT,
                           IProviderOfT_Get = tProviderOfT_Get,

                           InjectAttribute = tInjectAttribute,
                           ModuleAttribute = tModuleAttribute,
                           ProvidesAttribute = tProvidesAttribute,
                           NamedAttribute = tNamedAttribute,
                           SingletonAttribute = tSingletonAttribute,

                           ProcessedAssemblyAttribute = tProcessedAssemblyAttribute,
                           ProcessedAssemblyAttribute_Ctor = tProcessedAssemblyAttribute_Ctor
                       };
        }
    }
}
