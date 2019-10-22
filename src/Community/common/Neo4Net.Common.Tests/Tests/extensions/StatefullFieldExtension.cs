/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace Neo4Net.Test.extension
{
   using AfterAllCallback = org.junit.jupiter.api.extension.AfterAllCallback;
   using ExtensionContext = org.junit.jupiter.api.extension.ExtensionContext;
   using TestInstancePostProcessor = org.junit.jupiter.api.extension.TestInstancePostProcessor;

   public abstract class StatefullFieldExtension<T> : TestInstancePostProcessor, AfterAllCallback
   {
      protected internal abstract string FieldKey { get; }

      protected internal abstract Type<T> FieldType { get; }

      protected internal abstract T CreateField(ExtensionContext extensionContext);

      protected internal abstract ExtensionContext.Namespace NameSpace { get; }

      public override void AfterAll(ExtensionContext context)
      {
         RemoveStoredValue(context);
      }

      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: public void postProcessTestInstance(Object testInstance, org.junit.jupiter.api.extension.ExtensionContext context) throws Exception
      public override void PostProcessTestInstance(object testInstance, ExtensionContext context)
      {
         Type clazz = testInstance.GetType();
         object instance = CreateInstance(context);
         IList<System.Reflection.FieldInfo> declaredFields = GetAllFields(clazz);
         foreach (System.Reflection.FieldInfo declaredField in declaredFields)
         {
            if (declaredField.isAnnotationPresent(typeof(Inject)) && FieldType.Equals(declaredField.Type))
            {
               declaredField.Accessible = true;
               declaredField.set(testInstance, instance);
            }
         }
      }

      protected internal virtual T GetStoredValue(ExtensionContext context)
      {
         return GetLocalStore(context).get(FieldKey, FieldType);
      }

      internal virtual void RemoveStoredValue(ExtensionContext context)
      {
         GetLocalStore(context).remove(FieldKey, FieldType);
      }

      internal static ExtensionContext.Store GetStore(ExtensionContext extensionContext, ExtensionContext.Namespace @namespace)
      {
         return extensionContext.Root.getStore(@namespace);
      }

      private ExtensionContext.Store GetLocalStore(ExtensionContext extensionContext)
      {
         return GetStore(extensionContext, NameSpace);
      }

      private object CreateInstance(ExtensionContext extensionContext)
      {
         ExtensionContext.Store store = GetLocalStore(extensionContext);
         return store.getOrComputeIfAbsent(FieldKey, (System.Func<string, object>)s => CreateField(extensionContext));
      }

      private static IList<System.Reflection.FieldInfo> GetAllFields(Type baseClazz)
      {
         List<System.Reflection.FieldInfo> fields = new List<System.Reflection.FieldInfo>();
         Type clazz = baseClazz;
         do
         {
            Collections.addAll(fields, clazz.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance));
            clazz = clazz.BaseType;
         } while (clazz != null);
         return fields;
      }
   }
}