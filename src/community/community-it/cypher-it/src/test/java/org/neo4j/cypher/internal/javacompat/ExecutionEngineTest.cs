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
namespace Neo4Net.Cypher.Internal.javacompat
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using Result = Neo4Net.GraphDb.Result;
	using LoginContext = Neo4Net.Kernel.Api.Internal.security.LoginContext;
	using GraphDatabaseQueryService = Neo4Net.Kernel.GraphDatabaseQueryService;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using IPropertyContainerLocker = Neo4Net.Kernel.impl.coreapi.PropertyContainerLocker;
	using Neo4NetTransactionalContextFactory = Neo4Net.Kernel.impl.query.Neo4NetTransactionalContextFactory;
	using TransactionalContext = Neo4Net.Kernel.impl.query.TransactionalContext;
	using TransactionalContextFactory = Neo4Net.Kernel.impl.query.TransactionalContextFactory;
	using ClientConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.ClientConnectionInfo;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.EMPTY_MAP;

	public class ExecutionEngineTest
	{
		 private static readonly MapValue _noParams = VirtualValues.emptyMap();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.DatabaseRule database = new org.Neo4Net.test.rule.ImpermanentDatabaseRule();
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
			  IPropertyContainerLocker locker = new IPropertyContainerLocker();
			  TransactionalContextFactory contextFactory = Neo4NetTransactionalContextFactory.create( graph, locker );
			  return contextFactory.NewContext( ClientConnectionInfo.EMBEDDED_CONNECTION, tx, query, EMPTY_MAP );
		 }
	}

}