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
namespace Neo4Net.Kernel.impl.core
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using CleanupRule = Neo4Net.Test.rule.CleanupRule;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.assertion.Assert.assertObjectOrArrayEquals;

	public abstract class IPropertyContainerProxyTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.DatabaseRule db = new Neo4Net.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule Db = new ImpermanentDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.CleanupRule cleanup = new Neo4Net.test.rule.CleanupRule();
		 public readonly CleanupRule Cleanup = new CleanupRule();

		 protected internal abstract long CreatePropertyContainer();

		 protected internal abstract IPropertyContainer LookupPropertyContainer( long id );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllProperties()
		 public virtual void ShouldListAllProperties()
		 {
			  // Given
			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  properties["boolean"] = true;
			  properties["short_string"] = "abc";
			  properties["string"] = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVW" + "XYZabcdefghijklmnopqrstuvwxyz";
			  properties["long"] = long.MaxValue;
			  properties["short_array"] = new long[]{ 1, 2, 3, 4 };
			  properties["array"] = new long[]{ long.MaxValue - 1, long.MaxValue - 2, long.MaxValue - 3, long.MaxValue - 4, long.MaxValue - 5, long.MaxValue - 6, long.MaxValue - 7, long.MaxValue - 8, long.MaxValue - 9, long.MaxValue - 10, long.MaxValue - 11 };

			  long containerId;

			  using ( Transaction tx = Db.beginTx() )
			  {
					containerId = CreatePropertyContainer();
					PropertyContainer container = LookupPropertyContainer( containerId );

					foreach ( KeyValuePair<string, object> entry in properties.SetOfKeyValuePairs() )
					{
						 container.SetProperty( entry.Key, entry.Value );
					}

					tx.Success();
			  }

			  // When
			  IDictionary<string, object> listedProperties;
			  using ( Transaction tx = Db.beginTx() )
			  {
					listedProperties = LookupPropertyContainer( containerId ).AllProperties;
					tx.Success();
			  }

			  // Then
			  assertEquals( properties.Count, listedProperties.Count );
			  foreach ( string key in properties.Keys )
			  {
					assertObjectOrArrayEquals( properties[key], listedProperties[key] );
			  }
		 }
	}

}