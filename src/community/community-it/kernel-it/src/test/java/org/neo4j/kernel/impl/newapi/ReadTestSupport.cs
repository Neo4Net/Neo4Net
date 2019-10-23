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
namespace Neo4Net.Kernel.Impl.Newapi
{

	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Neo4Net.GraphDb.config;
	using GraphDatabaseBuilder = Neo4Net.GraphDb.factory.GraphDatabaseBuilder;
	using Kernel = Neo4Net.Kernel.Api.Internal.Kernel;
	using KernelAPIReadTestSupport = Neo4Net.Kernel.Api.Internal.KernelAPIReadTestSupport;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

	internal class ReadTestSupport : KernelAPIReadTestSupport
	{
		 private readonly IDictionary<Setting, string> _settings = new Dictionary<Setting, string>();
		 private IGraphDatabaseService _db;

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