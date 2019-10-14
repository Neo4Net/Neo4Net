using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.store.id.configuration
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class CommunityIdTypeConfigurationProviderTest
	public class CommunityIdTypeConfigurationProviderTest
	{

		 private IdType _reusableType;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters public static java.util.List<org.neo4j.kernel.impl.store.id.IdType> data()
		 public static IList<IdType> Data()
		 {
			  return Arrays.asList( IdType.PROPERTY, IdType.STRING_BLOCK, IdType.ARRAY_BLOCK, IdType.NODE_LABELS );
		 }

		 public CommunityIdTypeConfigurationProviderTest( IdType reusableType )
		 {
			  this._reusableType = reusableType;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nonReusableTypeConfiguration()
		 public virtual void NonReusableTypeConfiguration()
		 {
			  IdTypeConfigurationProvider provider = CreateIdTypeProvider();
			  IdTypeConfiguration typeConfiguration = provider.GetIdTypeConfiguration( IdType.RELATIONSHIP );
			  assertFalse( "Relationship ids are not reusable.", typeConfiguration.AllowAggressiveReuse() );
			  assertEquals( "Relationship ids are not reusable.", IdTypeConfiguration.DEFAULT_GRAB_SIZE, typeConfiguration.GrabSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void reusableTypeConfiguration()
		 public virtual void ReusableTypeConfiguration()
		 {
			  IdTypeConfigurationProvider provider = CreateIdTypeProvider();
			  IdTypeConfiguration typeConfiguration = provider.GetIdTypeConfiguration( _reusableType );
			  assertTrue( typeConfiguration.AllowAggressiveReuse() );
			  assertEquals( IdTypeConfiguration.AGGRESSIVE_GRAB_SIZE, typeConfiguration.GrabSize );
		 }

		 private IdTypeConfigurationProvider CreateIdTypeProvider()
		 {
			  return new CommunityIdTypeConfigurationProvider();
		 }

	}

}