﻿/*
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
namespace Org.Neo4j.Test.rule
{
	using GraphDatabaseBuilder = Org.Neo4j.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseFactory = Org.Neo4j.Graphdb.factory.GraphDatabaseFactory;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	/// <summary>
	/// JUnit @Rule for configuring, creating and managing an ImpermanentGraphDatabase instance.
	/// </summary>
	public class ImpermanentDatabaseRule : DatabaseRule
	{
		 private readonly LogProvider _userLogProvider;
		 private readonly LogProvider _internalLogProvider;

		 public ImpermanentDatabaseRule() : this(null)
		 {
		 }

		 public ImpermanentDatabaseRule( LogProvider logProvider )
		 {
			  this._userLogProvider = logProvider;
			  this._internalLogProvider = logProvider;
		 }

		 public override ImpermanentDatabaseRule StartLazily()
		 {
			  return ( ImpermanentDatabaseRule ) base.StartLazily();
		 }

		 protected internal override GraphDatabaseFactory NewFactory()
		 {
			  return MaybeSetInternalLogProvider( MaybeSetUserLogProvider( new TestGraphDatabaseFactory() ) );
		 }

		 protected internal override GraphDatabaseBuilder NewBuilder( GraphDatabaseFactory factory )
		 {
			  return ( ( TestGraphDatabaseFactory ) factory ).newImpermanentDatabaseBuilder();
		 }

		 protected internal TestGraphDatabaseFactory MaybeSetUserLogProvider( TestGraphDatabaseFactory factory )
		 {
			  return ( _userLogProvider == null ) ? factory : factory.setUserLogProvider( _userLogProvider );
		 }

		 protected internal TestGraphDatabaseFactory MaybeSetInternalLogProvider( TestGraphDatabaseFactory factory )
		 {
			  return ( _internalLogProvider == null ) ? factory : factory.setInternalLogProvider( _internalLogProvider );
		 }
	}

}