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
namespace Neo4Net.Test.rule
{
	using Description = org.junit.runner.Description;
	using Statement = org.junit.runners.model.Statement;

	using GraphDatabaseBuilder = Neo4Net.GraphDb.factory.GraphDatabaseBuilder;
	using GraphDatabaseFactory = Neo4Net.GraphDb.factory.GraphDatabaseFactory;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;


	/// <summary>
	/// JUnit @Rule for configuring, creating and managing an EmbeddedGraphDatabase instance.
	/// <para>
	/// The database instance is created lazily, so configurations can be injected prior to calling
	/// <seealso cref="getGraphDatabaseAPI()"/>.
	/// </para>
	/// </summary>
	public class EmbeddedDatabaseRule : DatabaseRule
	{
		 private readonly TestDirectory _testDirectory;

		 public EmbeddedDatabaseRule()
		 {
			  this._testDirectory = TestDirectory.TestDirectoryConflict();
		 }

		 public EmbeddedDatabaseRule( TestDirectory testDirectory )
		 {
			  this._testDirectory = testDirectory;
		 }

		 public override EmbeddedDatabaseRule StartLazily()
		 {
			  return ( EmbeddedDatabaseRule ) base.StartLazily();
		 }

		 public override DatabaseLayout DatabaseLayout()
		 {
			  return _testDirectory.databaseLayout();
		 }

		 protected internal override GraphDatabaseFactory NewFactory()
		 {
			  return new TestGraphDatabaseFactory();
		 }

		 protected internal override GraphDatabaseBuilder NewBuilder( GraphDatabaseFactory factory )
		 {
			  return factory.NewEmbeddedDatabaseBuilder( _testDirectory.databaseDir() );
		 }

		 public override Statement Apply( Statement @base, Description description )
		 {
			  return _testDirectory.apply( base.Apply( @base, description ), description );
		 }

	}

}