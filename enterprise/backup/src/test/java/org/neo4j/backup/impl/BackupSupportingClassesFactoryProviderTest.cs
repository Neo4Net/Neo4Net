using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.backup.impl
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.backup.impl.BackupSupportingClassesFactoryProvider.getProvidersByPriority;

	public class BackupSupportingClassesFactoryProviderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canLoadDefaultSupportingClassesFactory()
		 public virtual void CanLoadDefaultSupportingClassesFactory()
		 {

			  assertEquals( 1, FindInstancesOf( typeof( BackupSupportingClassesFactoryProvider ), AllAvailableSupportingClassesFactories() ).Count );

			  assertEquals( 1, FindInstancesOf( typeof( OpenEnterpriseBackupSupportingClassesFactoryProvider ), AllAvailableSupportingClassesFactories() ).Count );

			  // Note that the allAvailableSupportingClassesFactories() also loads the org.neo4j.helpers.Service[].
			  assertEquals( 3, AllAvailableSupportingClassesFactories().Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDefaultModuleIsPrioritisedOverDummyModule()
		 public virtual void TestDefaultModuleIsPrioritisedOverDummyModule()
		 {
			  assertEquals( typeof( OpenEnterpriseBackupSupportingClassesFactoryProvider ), ProvidersByPriority.findFirst().get().GetType() );
		 }

		 public static ICollection<BackupSupportingClassesFactoryProvider> AllAvailableSupportingClassesFactories()
		 {
			  return ProvidersByPriority.collect( toList() );
		 }

		 /// <summary>
		 /// T
		 /// </summary>
		 /// <param name="desiredClass"> </param>
		 /// <param name="collection"> </param>
		 /// @param <DESIRED>
		 /// @return </param>
		 public static ICollection<DESIRED> FindInstancesOf<DESIRED, T1>( Type desiredClass, ICollection<T1> collection ) where DESIRED : BackupSupportingClassesFactoryProvider where T1 : BackupSupportingClassesFactoryProvider
		 {
				 desiredClass = typeof( DESIRED );
			  return collection.Where( IsOfClass( desiredClass ) ).Select( i => ( DESIRED ) i ).ToList();
		 }

		 /// <summary>
		 /// Note: The method name here may be confusing.  It does not check if a class extended another class - only if the class name is an exact match.
		 /// </summary>
		 /// <param name="desiredClass">
		 /// @return </param>
		 private static System.Predicate<BackupSupportingClassesFactoryProvider> IsOfClass( Type desiredClass )
		 {
			  return factory => desiredClass.Equals( factory.GetType() );
		 }
	}

}