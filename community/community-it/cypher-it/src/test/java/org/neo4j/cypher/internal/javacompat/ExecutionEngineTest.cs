/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Cypher.@internal.javacompat
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using Result = Org.Neo4j.Graphdb.Result;
	using LoginContext = Org.Neo4j.@internal.Kernel.Api.security.LoginContext;
	using GraphDatabaseQueryService = Org.Neo4j.Kernel.GraphDatabaseQueryService;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using InternalTransaction = Org.Neo4j.Kernel.impl.coreapi.InternalTransaction;
	using PropertyContainerLocker = Org.Neo4j.Kernel.impl.coreapi.PropertyContainerLocker;
	using Neo4jTransactionalContextFactory = Org.Neo4j.Kernel.impl.query.Neo4jTransactionalContextFactory;
	using TransactionalContext = Org.Neo4j.Kernel.impl.query.TransactionalContext;
	using TransactionalContextFactory = Org.Neo4j.Kernel.impl.query.TransactionalContextFactory;
	using ClientConnectionInfo = Org.Neo4j.Kernel.impl.query.clientconnection.ClientConnectionInfo;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;
	using VirtualValues = Org.Neo4j.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_MAP;

	public class ExecutionEngineTest
	{
		 private static readonly MapValue _noParams = VirtualValues.emptyMap();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.DatabaseRule database = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public DatabaseRule Database = new ImpermanentDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConvertListsAndMapsWhenPassingFromScalaToJava() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldConvertListsAndMapsWhenPassingFromScalaToJava()
		 {
			  GraphDatabaseQueryService graph = new GraphDatabaseCypherService( this.Database.GraphDatabaseAPI );
			  DependencyResolver resolver = graph.DependencyResolver;
			  Monitors monitors = resolver.ResolveDependency( typeof( Monitors ) );

			  NullLogProvider nullLogProvider = NullLogProvider.Instance;

			  Config config = resolver.ResolveDependency( typeof( Config ) );
			  CypherConfiguration cypherConfig = CypherConfiguration.fromConfig( config );

			  CommunityCompilerFactory compilerFactory = new CommunityCompilerFactory( graph, monitors, nullLogProvider, cypherConfig.toCypherPlannerConfiguration( config ), cypherConfig.toCypherRuntimeConfiguration() );
			  ExecutionEngine executionEngine = new ExecutionEngine( graph, nullLogProvider, compilerFactory );

			  Result result;
			  using ( InternalTransaction tx = graph.BeginTransaction( KernelTransaction.Type.@implicit, LoginContext.AUTH_DISABLED ) )
			  {
					string query = "RETURN { key : 'Value' , collectionKey: [{ inner: 'Map1' }, { inner: 'Map2' }]}";
					TransactionalContext tc = CreateTransactionContext( graph, tx, query );
					result = executionEngine.ExecuteQuery( query, _noParams, tc );

					VerifyResult( result );

					result.Close();
					tx.Success();
			  }
		 }

		 private void VerifyResult( Result result )
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  System.Collections.IDictionary firstRowValue = ( System.Collections.IDictionary ) result.Next().Values.GetEnumerator().next();
			  assertThat( firstRowValue["key"], @is( "Value" ) );
			  System.Collections.IList theList = ( System.Collections.IList ) firstRowValue["collectionKey"];
			  assertThat( ( ( System.Collections.IDictionary ) theList[0] )["inner"], @is( "Map1" ) );
			  assertThat( ( ( System.Collections.IDictionary ) theList[1] )["inner"], @is( "Map2" ) );
		 }

		 private TransactionalContext CreateTransactionContext( GraphDatabaseQueryService graph, InternalTransaction tx, string query )
		 {
			  PropertyContainerLocker locker = new PropertyContainerLocker();
			  TransactionalContextFactory contextFactory = Neo4jTransactionalContextFactory.create( graph, locker );
			  return contextFactory.NewContext( ClientConnectionInfo.EMBEDDED_CONNECTION, tx, query, EMPTY_MAP );
		 }
	}

}