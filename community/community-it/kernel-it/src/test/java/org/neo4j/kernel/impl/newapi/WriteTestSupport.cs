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
	using PropertyContainer = Org.Neo4j.Graphdb.PropertyContainer;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Kernel = Org.Neo4j.@internal.Kernel.Api.Kernel;
	using KernelAPIWriteTestSupport = Org.Neo4j.@internal.Kernel.Api.KernelAPIWriteTestSupport;
	using EmbeddedProxySPI = Org.Neo4j.Kernel.impl.core.EmbeddedProxySPI;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using GraphDatabaseServiceCleaner = Org.Neo4j.Test.GraphDatabaseServiceCleaner;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;

	internal class WriteTestSupport : KernelAPIWriteTestSupport
	{
		 private GraphDatabaseService _db;

		 public override void Setup( File storeDir )
		 {
			  _db = NewDb( storeDir );
		 }

		 protected internal virtual GraphDatabaseService NewDb( File storeDir )
		 {
			  return ( new TestGraphDatabaseFactory() ).newImpermanentDatabaseBuilder(storeDir).newGraphDatabase();
		 }

		 public override void ClearGraph()
		 {
			  GraphDatabaseServiceCleaner.cleanDatabaseContent( _db );
			  using ( Transaction tx = _db.beginTx() )
			  {
					PropertyContainer graphProperties = graphProperties();
					foreach ( string key in graphProperties.PropertyKeys )
					{
						 graphProperties.RemoveProperty( key );
					}
					tx.Success();
			  }
		 }

		 public override PropertyContainer GraphProperties()
		 {
			  return ( ( GraphDatabaseAPI ) _db ).DependencyResolver.resolveDependency( typeof( EmbeddedProxySPI ) ).newGraphPropertiesProxy();
		 }

		 public override Kernel KernelToTest()
		 {
			  DependencyResolver resolver = ( ( GraphDatabaseAPI ) this._db ).DependencyResolver;
			  return resolver.ResolveDependency( typeof( Kernel ) );
		 }

		 public override GraphDatabaseService GraphBackdoor()
		 {
			  return _db;
		 }

		 public override void TearDown()
		 {
			  _db.shutdown();
			  _db = null;
		 }
	}

}