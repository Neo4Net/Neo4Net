using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.management.impl
{
	using Test = org.junit.Test;

	using Kernel = Neo4Net.Jmx.Kernel;
	using Primitives = Neo4Net.Jmx.Primitives;
	using StoreFile = Neo4Net.Jmx.StoreFile;
	using ManagementSupport = Neo4Net.Jmx.impl.ManagementSupport;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class CodeDuplicationValidationTest
	{
		 private class DefaultManagementSupport : ManagementSupport
		 {
			 private readonly CodeDuplicationValidationTest _outerInstance;

			 public DefaultManagementSupport( CodeDuplicationValidationTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  protected internal override ObjectName CreateObjectName( string instanceId, string beanName, bool query, params string[] extraNaming )
			  {
					return base.CreateObjectName( instanceId, beanName, query, extraNaming );
			  }

			  protected internal override string GetBeanName( Type beanInterface )
			  {
					return base.GetBeanName( beanInterface );
			  }
		 }

		 private class CustomManagementSupport : AdvancedManagementSupport
		 {
			 private readonly CodeDuplicationValidationTest _outerInstance;

			 public CustomManagementSupport( CodeDuplicationValidationTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  // belongs to this package - no override needed
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void kernelBeanTypeNameMatchesExpected()
		 public virtual void KernelBeanTypeNameMatchesExpected()
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  assertEquals( typeof( Kernel ).FullName, KernelProxy.KERNEL_BEAN_TYPE );
			  assertEquals( Neo4Net.Jmx.Kernel_Fields.NAME, KernelProxy.KERNEL_BEAN_NAME );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mbeanQueryAttributeNameMatchesMethodName() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MbeanQueryAttributeNameMatchesMethodName()
		 {
			  assertEquals( typeof( ObjectName ), typeof( Kernel ).GetMethod( "get" + KernelProxy.MBEAN_QUERY ).ReturnType );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void interfacesGetsTheSameBeanNames()
		 public virtual void InterfacesGetsTheSameBeanNames()
		 {
			  AssertEqualBeanName( typeof( Kernel ) );
			  AssertEqualBeanName( typeof( Primitives ) );
			  AssertEqualBeanName( typeof( HighAvailability ) );
			  AssertEqualBeanName( typeof( BranchedStore ) );
			  AssertEqualBeanName( typeof( LockManager ) );
			  AssertEqualBeanName( typeof( MemoryMapping ) );
			  AssertEqualBeanName( typeof( StoreFile ) );
			  AssertEqualBeanName( typeof( TransactionManager ) );
			  AssertEqualBeanName( typeof( IndexSamplingManager ) );
		 }

		 private void AssertEqualBeanName( Type beanClass )
		 {
			  assertEquals( ( new DefaultManagementSupport( this ) ).GetBeanName( beanClass ), ( new CustomManagementSupport( this ) ).GetBeanName( beanClass ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void generatesEqualObjectNames()
		 public virtual void GeneratesEqualObjectNames()
		 {
			  assertEquals( ( new DefaultManagementSupport( this ) ).CreateMBeanQuery( "test-instance" ), ( new CustomManagementSupport( this ) ).CreateMBeanQuery( "test-instance" ) );
			  assertEquals( ( new DefaultManagementSupport( this ) ).CreateObjectName( "test-instance", typeof( Kernel ) ), ( new CustomManagementSupport( this ) ).CreateObjectName( "test-instance", typeof( Kernel ) ) );
		 }
	}

}