using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo Technology,"
 * Network Engine for Objects in Lund AB [http://neotechnology.com]
 *
 * Modifications Copyright (c) 2018-2019 "GraphFoundation" [https://graphfoundation.org]
 *
 * The included source code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html)
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 */

namespace Neo4Net.backup.impl
{

	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.backup.impl.OpenEnterpriseBackupSupportingClassesFactoryProvider.getProvidersByPriority;

	// See: https://github.com/Neo4Net/Neo4Net/blob/3.4/enterprise/backup/src/test/java/org/Neo4Net/backup/impl/BackupSupportingClassesFactoryProviderTest.java
	public class OpenEnterpriseBackupSupportingClassesFactoryProviderTest
	{
		 public static ICollection<BackupSupportingClassesFactoryProvider> AllAvailableSupportingClassesFactories()
		 {
			  return ProvidersByPriority.collect( toList() );
		 }

		 public static ICollection<DESIRED> FindInstancesOf<DESIRED, T1>( Type desiredClass, ICollection<T1> collection ) where DESIRED : BackupSupportingClassesFactoryProvider where T1 : BackupSupportingClassesFactoryProvider
		 {
				 desiredClass = typeof( DESIRED );
			  return collection.Where( IsOfClass( desiredClass ) ).Select( i => ( DESIRED ) i ).ToList();
		 }

		 private static System.Predicate<BackupSupportingClassesFactoryProvider> IsOfClass( Type desiredClass )
		 {
			  return factory => desiredClass.Equals( factory.GetType() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canLoadDefaultSupportingClassesFactory()
		 public virtual void CanLoadDefaultSupportingClassesFactory()
		 {

			  assertEquals( 1, FindInstancesOf( typeof( OpenEnterpriseBackupSupportingClassesFactoryProvider ), AllAvailableSupportingClassesFactories() ).Count );
			  assertEquals( 3, AllAvailableSupportingClassesFactories().Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDefaultModuleIsPrioritisedOverDummyModule()
		 public virtual void TestDefaultModuleIsPrioritisedOverDummyModule()
		 {
			  assertEquals( typeof( OpenEnterpriseBackupSupportingClassesFactoryProvider ), ProvidersByPriority.findFirst().get().GetType() );
		 }
	}

}