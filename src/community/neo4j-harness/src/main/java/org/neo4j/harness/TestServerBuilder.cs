using System;

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
namespace Neo4Net.Harness
{

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Neo4Net.GraphDb.config;
	using Procedure = Neo4Net.Procedure.Procedure;
	using UserAggregationFunction = Neo4Net.Procedure.UserAggregationFunction;
	using UserFunction = Neo4Net.Procedure.UserFunction;

	/// <summary>
	/// Utility for constructing and starting Neo4Net for test purposes.
	/// </summary>
	public interface TestServerBuilder
	{
		 /// <summary>
		 /// Start a new server. By default, the server will listen to a random free port, and you can determine where to
		 /// connect using the <seealso cref="ServerControls.httpURI()"/> method. You could also specify explicit ports using the
		 /// <seealso cref="withConfig(org.Neo4Net.graphdb.config.Setting, string)"/> method. Please refer to the Neo4Net Manual for
		 /// details on available configuration options.
		 /// 
		 /// When the returned controls are <seealso cref="ServerControls.close() closed"/>, the temporary directory the server used
		 /// will be removed as well.
		 /// </summary>
		 ServerControls NewServer();

		 /// <summary>
		 /// Configure the Neo4Net instance. Configuration here can be both configuration aimed at the server as well as the
		 /// database tuning options. Please refer to the Neo4Net Manual for details on available configuration options.
		 /// </summary>
		 /// <param name="key"> the config key </param>
		 /// <param name="value"> the config value </param>
		 /// <returns> this builder instance </returns>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: TestServerBuilder withConfig(org.Neo4Net.graphdb.config.Setting<?> key, String value);
		 TestServerBuilder withConfig<T1>( Setting<T1> key, string value );

		 /// <seealso cref= #withConfig(org.Neo4Net.graphdb.config.Setting, String) </seealso>
		 TestServerBuilder WithConfig( string key, string value );

		 /// <summary>
		 /// Shortcut for configuring the server to use an unmanaged extension. Please refer to the Neo4Net Manual on how to
		 /// write unmanaged extensions.
		 /// </summary>
		 /// <param name="mountPath"> the http path, relative to the server base URI, that this extension should be mounted at. </param>
		 /// <param name="extension"> the extension class. </param>
		 /// <returns> this builder instance </returns>
		 TestServerBuilder WithExtension( string mountPath, Type extension );

		 /// <summary>
		 /// Shortcut for configuring the server to find and mount all unmanaged extensions in the given package. </summary>
		 /// <seealso cref= #withExtension(String, Class) </seealso>
		 /// <param name="mountPath"> the http path, relative to the server base URI, that this extension should be mounted at. </param>
		 /// <param name="packageName"> a java package with extension classes. </param>
		 /// <returns> this builder instance </returns>
		 TestServerBuilder WithExtension( string mountPath, string packageName );

		 /// <summary>
		 /// Data fixtures to inject upon server start. This can be either a file with a plain-text cypher query
		 /// (for example, myFixture.cyp), or a directory containing such files with the suffix ".cyp". </summary>
		 /// <param name="cypherFileOrDirectory"> file with cypher statement, or directory containing ".cyp"-suffixed files. </param>
		 /// <returns> this builder instance </returns>
		 TestServerBuilder WithFixture( File cypherFileOrDirectory );

		 /// <summary>
		 /// Data fixture to inject upon server start. This should be a valid Cypher statement. </summary>
		 /// <param name="fixtureStatement"> a cypher statement </param>
		 /// <returns> this builder instance </returns>
		 TestServerBuilder WithFixture( string fixtureStatement );

		 /// <summary>
		 /// Data fixture to inject upon server start. This should be a user implemented fixture function
		 /// operating on a <seealso cref="GraphDatabaseService"/> instance </summary>
		 /// <param name="fixtureFunction"> a fixture function </param>
		 /// <returns> this builder instance </returns>
		 TestServerBuilder WithFixture( System.Func<GraphDatabaseService, Void> fixtureFunction );

		 /// <summary>
		 /// Pre-populate the server with databases copied from the specified source directory.
		 /// The source directory needs to have sub-folders `databases/graph.db` in which the source store files are located. </summary>
		 /// <param name="sourceDirectory"> the directory to copy from </param>
		 /// <returns> this builder instance </returns>
		 TestServerBuilder CopyFrom( File sourceDirectory );

		 /// <summary>
		 /// Configure the server to load the specified procedure definition class. The class should contain one or more
		 /// methods annotated with <seealso cref="Procedure"/>, these will become available to call through
		 /// cypher.
		 /// </summary>
		 /// <param name="procedureClass"> a class containing one or more procedure definitions </param>
		 /// <returns> this builder instance </returns>
		 TestServerBuilder WithProcedure( Type procedureClass );

		 /// <summary>
		 /// Configure the server to load the specified function definition class. The class should contain one or more
		 /// methods annotated with <seealso cref="UserFunction"/>, these will become available to call through
		 /// cypher.
		 /// </summary>
		 /// <param name="functionClass"> a class containing one or more function definitions </param>
		 /// <returns> this builder instance </returns>
		 TestServerBuilder WithFunction( Type functionClass );

		 /// <summary>
		 /// Configure the server to load the specified aggregation function definition class. The class should contain one or more
		 /// methods annotated with <seealso cref="UserAggregationFunction"/>, these will become available to call through
		 /// cypher.
		 /// </summary>
		 /// <param name="functionClass"> a class containing one or more function definitions </param>
		 /// <returns> this builder instance </returns>
		 TestServerBuilder WithAggregationFunction( Type functionClass );
	}

}