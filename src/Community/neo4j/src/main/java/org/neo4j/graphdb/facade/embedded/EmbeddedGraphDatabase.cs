using System.Collections.Generic;

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
namespace Neo4Net.Graphdb.facade.embedded
{

	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using CommunityEditionModule = Neo4Net.Graphdb.factory.module.edition.CommunityEditionModule;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.facade.GraphDatabaseDependencies.newDependencies;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.append;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asList;

	/// <summary>
	/// An implementation of <seealso cref="GraphDatabaseService"/> that is used to embed Neo4j
	/// in an application. You typically instantiate it by using
	/// <seealso cref="org.neo4j.graphdb.factory.GraphDatabaseFactory"/> like so:
	/// <para>
	/// 
	/// <pre>
	/// <code>
	/// GraphDatabaseService graphDb = new GraphDatabaseFactory().newEmbeddedDatabase( &quot;var/graphdb&quot; );
	/// // ... use Neo4j
	/// graphDb.shutdown();
	/// </code>
	/// </pre>
	/// </para>
	/// <para>
	/// For more information, see <seealso cref="GraphDatabaseService"/>.
	/// </para>
	/// </summary>
	public class EmbeddedGraphDatabase : GraphDatabaseFacade
	{
		 /// <summary>
		 /// Internal constructor used by <seealso cref="org.neo4j.graphdb.factory.GraphDatabaseFactory"/>
		 /// </summary>
		 public EmbeddedGraphDatabase( File storeDir, IDictionary<string, string> @params, GraphDatabaseFacadeFactory.Dependencies dependencies )
		 {
			  Create( storeDir, @params, dependencies );
		 }

		 /// <summary>
		 /// Internal constructor used by ImpermanentGraphDatabase
		 /// </summary>
		 protected internal EmbeddedGraphDatabase( File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies )
		 {
			  Create( storeDir, config, dependencies );
		 }

		 protected internal virtual void Create( File storeDir, IDictionary<string, string> @params, GraphDatabaseFacadeFactory.Dependencies dependencies )
		 {
			  GraphDatabaseDependencies newDependencies = newDependencies( dependencies ).settingsClasses( asList( append( typeof( GraphDatabaseSettings ), dependencies.SettingsClasses() ) ) );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  ( new GraphDatabaseFacadeFactory( DatabaseInfo.COMMUNITY, CommunityEditionModule::new ) ).initFacade( storeDir, @params, newDependencies, this );
		 }

		 protected internal virtual void Create( File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies )
		 {
			  GraphDatabaseDependencies newDependencies = newDependencies( dependencies ).settingsClasses( asList( append( typeof( GraphDatabaseSettings ), dependencies.SettingsClasses() ) ) );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  ( new GraphDatabaseFacadeFactory( DatabaseInfo.COMMUNITY, CommunityEditionModule::new ) ).initFacade( storeDir, config, newDependencies, this );
		 }
	}

}