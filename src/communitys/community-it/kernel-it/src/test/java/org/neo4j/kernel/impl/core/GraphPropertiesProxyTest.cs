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
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using GraphTransactionRule = Neo4Net.Test.rule.GraphTransactionRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class GraphPropertiesProxyTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public static DatabaseRule Db = new ImpermanentDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.GraphTransactionRule tx = new org.neo4j.test.rule.GraphTransactionRule(db);
		 public GraphTransactionRule Tx = new GraphTransactionRule( Db );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGraphAddPropertyWithNullKey()
		 public virtual void TestGraphAddPropertyWithNullKey()
		 {
			  try
			  {
					GraphProperties().setProperty(null, "bar");
					fail( "Null key should result in exception." );
			  }
			  catch ( System.ArgumentException )
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGraphAddPropertyWithNullValue()
		 public virtual void TestGraphAddPropertyWithNullValue()
		 {
			  try
			  {
					GraphProperties().setProperty("foo", null);
					fail( "Null value should result in exception." );
			  }
			  catch ( System.ArgumentException )
			  {
			  }
			  Tx.failure();
		 }

		 private GraphProperties GraphProperties()
		 {
			  return Db.DependencyResolver.resolveDependency( typeof( EmbeddedProxySPI ) ).newGraphPropertiesProxy();
		 }
	}

}