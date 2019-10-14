using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Cypher.Internal.javacompat
{
	using Test = org.junit.jupiter.api.Test;

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.anyOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;

	internal class ExpressionEngineConfigurationTest
	{
		 private readonly AssertableLogProvider _logProvider = new AssertableLogProvider();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBeUsingInterpretedByDefault()
		 internal virtual void ShouldBeUsingInterpretedByDefault()
		 {
			  // Given
			  string query = "RETURN sin(cos(sin(cos(rand()))))";
			  GraphDatabaseService db = WithEngineAndLimit( "DEFAULT", 0 );
			  int manyTimes = 10;
			  for ( int i = 0; i < manyTimes; i++ )
			  {
					AssertNotUsingCompiled( db, query );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotUseCompiledExpressionsFirstTimeWithJitEnabled()
		 internal virtual void ShouldNotUseCompiledExpressionsFirstTimeWithJitEnabled()
		 {
			  AssertNotUsingCompiled( WithEngineAndLimit( "ONLY_WHEN_HOT", 1 ), "RETURN sin(cos(sin(cos(rand()))))" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUseCompiledExpressionsFirstTimeWhenLimitIsZero()
		 internal virtual void ShouldUseCompiledExpressionsFirstTimeWhenLimitIsZero()
		 {
			  AssertUsingCompiled( WithEngineAndLimit( "ONLY_WHEN_HOT", 0 ), "RETURN sin(cos(sin(cos(rand()))))" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUseCompiledExpressionsWhenQueryIsHotWithJitEnabled()
		 internal virtual void ShouldUseCompiledExpressionsWhenQueryIsHotWithJitEnabled()
		 {
			  // Given
			  string query = "RETURN sin(cos(sin(cos(rand()))))";
			  GraphDatabaseService db = WithEngineAndLimit( "ONLY_WHEN_HOT", 3 );

			  // When
			  Db.execute( query );
			  Db.execute( query );
			  Db.execute( query );

			  // Then
			  AssertUsingCompiled( db, query );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUseCompiledExpressionsFirstTimeWhenConfigured()
		 internal virtual void ShouldUseCompiledExpressionsFirstTimeWhenConfigured()
		 {
			  AssertUsingCompiled( WithEngineAndLimit( "COMPILED", 42 ), "RETURN sin(cos(sin(cos(rand()))))" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUseCompiledExpressionsFirstTimeWhenExplicitlyAskedFor()
		 internal virtual void ShouldUseCompiledExpressionsFirstTimeWhenExplicitlyAskedFor()
		 {
			  AssertUsingCompiled( WithEngineAndLimit( "ONLY_WHEN_HOT", 42 ), "CYPHER expressionEngine=COMPILED RETURN sin(cos(sin(cos(rand()))))" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotUseCompiledExpressionsWhenExplicitlyAskingForInterpreted()
		 internal virtual void ShouldNotUseCompiledExpressionsWhenExplicitlyAskingForInterpreted()
		 {
			  AssertNotUsingCompiled( WithEngineAndLimit( "COMPILED", 42 ), "CYPHER expressionEngine=INTERPRETED RETURN sin(cos(sin(cos(rand()))))" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUseCompiledExpressionsEvenIfPotentiallyCached()
		 internal virtual void ShouldUseCompiledExpressionsEvenIfPotentiallyCached()
		 {
			  // Given
			  string query = "RETURN sin(cos(sin(cos(rand()))))";
			  GraphDatabaseService db = WithEngineAndLimit( "INTERPRETED", 0 );

			  // When
			  Db.execute( query );

			  // Then
			  AssertUsingCompiled( db, "CYPHER expressionEngine=COMPILED " + query );
		 }

		 private GraphDatabaseService WithEngineAndLimit( string engine, int limit )
		 {

			  return ( new TestGraphDatabaseFactory() ).setInternalLogProvider(_logProvider).newImpermanentDatabaseBuilder().setConfig(GraphDatabaseSettings.cypher_expression_engine, engine).setConfig(GraphDatabaseSettings.cypher_expression_recompilation_limit, Convert.ToString(limit)).newGraphDatabase();
		 }

		 private void AssertUsingCompiled( GraphDatabaseService db, string query )
		 {
			  _logProvider.clear();
			  Db.execute( query ).resultAsString();

			  _logProvider.assertAtLeastOnce( inLog( typeof( EnterpriseCompilerFactory ) ).debug( anyOf( containsString( "Compiling expression:" ), containsString( "Compiling projection:" ) ) ) );
		 }

		 private void AssertNotUsingCompiled( GraphDatabaseService db, string query )
		 {
			  _logProvider.clear();
			  Db.execute( query ).resultAsString();

			  _logProvider.assertNone( inLog( typeof( EnterpriseCompilerFactory ) ).debug( anyOf( containsString( "Compiling expression:" ), containsString( "Compiling projection:" ) ) ) );
		 }

	}

}