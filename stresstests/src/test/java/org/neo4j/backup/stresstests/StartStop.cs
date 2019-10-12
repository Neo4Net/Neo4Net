/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.backup.stresstests
{

	using Control = Org.Neo4j.causalclustering.stresstests.Control;
	using Org.Neo4j.Function;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Workload = Org.Neo4j.helper.Workload;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	internal class StartStop : Workload
	{
		 private readonly AtomicReference<GraphDatabaseService> _dbRef;
		 private readonly Factory<GraphDatabaseService> _factory;

		 internal StartStop( Control control, Factory<GraphDatabaseService> factory, AtomicReference<GraphDatabaseService> dbRef ) : base( control )
		 {
			  this._factory = factory;
			  this._dbRef = dbRef;
		 }

		 protected internal override void DoWork()
		 {
			  GraphDatabaseService db = _dbRef.get();
			  Db.shutdown();
			  LockSupport.parkNanos( TimeUnit.SECONDS.toNanos( 5 ) );
			  bool replaced = _dbRef.compareAndSet( db, _factory.newInstance() );
			  assertTrue( replaced );
		 }
	}

}