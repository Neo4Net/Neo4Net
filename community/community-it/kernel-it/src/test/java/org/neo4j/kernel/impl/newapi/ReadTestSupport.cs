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
namespace Org.Neo4j.Kernel.Impl.Newapi
{

	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Org.Neo4j.Graphdb.config;
	using GraphDatabaseBuilder = Org.Neo4j.Graphdb.factory.GraphDatabaseBuilder;
	using Kernel = Org.Neo4j.@internal.Kernel.Api.Kernel;
	using KernelAPIReadTestSupport = Org.Neo4j.@internal.Kernel.Api.KernelAPIReadTestSupport;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;

	internal class ReadTestSupport : KernelAPIReadTestSupport
	{
		 private readonly IDictionary<Setting, string> _settings = new Dictionary<Setting, string>();
		 private GraphDatabaseService _db;

		 internal virtual void AddSetting( Setting setting, string value )
		 {
			  _settings[setting] = value;
		 }

		 public override void Setup( File storeDir, System.Action<GraphDatabaseService> create )
		 {
			  GraphDatabaseBuilder graphDatabaseBuilder = ( new TestGraphDatabaseFactory() ).newImpermanentDatabaseBuilder(storeDir);
			  _settings.forEach( graphDatabaseBuilder.setConfig );
			  _db = graphDatabaseBuilder.NewGraphDatabase();
			  create( _db );
		 }

		 public override Kernel KernelToTest()
		 {
			  DependencyResolver resolver = ( ( GraphDatabaseAPI ) this._db ).DependencyResolver;
			  return resolver.ResolveDependency( typeof( Kernel ) );
		 }

		 public override void TearDown()
		 {
			  _db.shutdown();
			  _db = null;
		 }
	}

}