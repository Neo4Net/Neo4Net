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
 *
 * ******************************************
 * Portions Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 * ******************************************
 */

//
using NSubstitute;

using System;
using Xunit;

namespace Neo4Net.Functions
{
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.hamcrest.MatcherAssert.assertThat;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.hamcrest.Matchers.sameInstance;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.junit.jupiter.api.Assertions.assertFalse;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.junit.jupiter.api.Assertions.assertTrue;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.mockito.Mockito.mock;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.mockito.Mockito.verify;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.mockito.Mockito.verifyNoMoreInteractions;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.mockito.Mockito.verifyZeroInteractions;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.mockito.Mockito.when;

   public class SuppliersTest
   {
      [Fact]
      public void SingletonSupplierShouldAlwaysReturnSame()
      {
         object o = new object();
         System.Func<object> supplier = Suppliers.Singleton(o);

         Assert.True(Object.ReferenceEquals(supplier(), o));
         Assert.True(Object.ReferenceEquals(supplier(), o));
         Assert.True(Object.ReferenceEquals(supplier(), o));
      }

      [Fact]
      public void LazySingletonSupplierShouldOnlyRequestInstanceWhenRequired()
      {
         object o = new object();
         System.Func<object> mockSupplier = Substitute.For<System.Func<object>>(); //$!!$ mock(typeof(System.Func));
         mockSupplier().Returns(o); // $!!$ tac when(mockSupplier()).thenReturn(o);

         System.Func<object> supplier = Suppliers.LazySingleton(mockSupplier) ;
         //Lazy<object> supplier = Suppliers.LazySingleton<object>(mockSupplier);
         

         //$$!$ verifyZeroInteractions(mockSupplier);

         Assert.True(Object.ReferenceEquals(supplier(), o));
         Assert.True(Object.ReferenceEquals(supplier(), o));
         Assert.True(Object.ReferenceEquals(supplier(), o));

         verify(mockSupplier).get();
         verifyNoMoreInteractions(mockSupplier);
      }

      
      [Fact]
      public void AdaptedSupplierShouldOnlyCallAdaptorOnceForEachNewInstance()
      {
         object o1 = new object();
         object o1a = new object();
         object o2 = new object();
         object o2a = new object();
         object o3 = new object();
         object o3a = new object();
         System.Func<object> mockSupplier = Substitute.For<System.Func<object>>();
         mockSupplier.When(mockSupplier()).thenReturn(o1, o1, o1, o2, o3, o3);

         System.Func<object, object> mockFunction = mock(typeof(System.Func));
         when(mockFunction(o1)).thenReturn(o1a);
         when(mockFunction(o2)).thenReturn(o2a);
         when(mockFunction(o3)).thenReturn(o3a);

         System.Func<object> supplier = Suppliers.Adapted(mockSupplier, mockFunction);

         assertThat(supplier(), sameInstance(o1a));
         assertThat(supplier(), sameInstance(o1a));
         assertThat(supplier(), sameInstance(o1a));
         assertThat(supplier(), sameInstance(o2a));
         assertThat(supplier(), sameInstance(o3a));
         assertThat(supplier(), sameInstance(o3a));

         verify(mockFunction).apply(o1);
         verify(mockFunction).apply(o2);
         verify(mockFunction).apply(o3);
         verifyNoMoreInteractions(mockFunction);
      }

      
      [Fact] 
      public void CorrectlyReportNotInitializedSuppliers()
      {
         //JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
         Suppliers.Lazy<object> lazySingleton = Suppliers.LazySingleton(object::new );
         assertFalse(lazySingleton.Initialized);
      }

      
      [Fact]
      public void CorrectlyReportInitializedSuppliers()
      {
         //JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
         Suppliers.Lazy<object> lazySingleton = Suppliers.LazySingleton(object::new );
         lazySingleton.get();
         Assert.True(lazySingleton.Initialized);
      }
   }
}