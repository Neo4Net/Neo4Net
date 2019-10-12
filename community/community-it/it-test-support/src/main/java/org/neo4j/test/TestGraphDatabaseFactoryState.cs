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
namespace Org.Neo4j.Test
{
	using GraphDatabaseFactoryState = Org.Neo4j.Graphdb.factory.GraphDatabaseFactoryState;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using SystemNanoClock = Org.Neo4j.Time.SystemNanoClock;

	public class TestGraphDatabaseFactoryState : GraphDatabaseFactoryState
	{
		 private FileSystemAbstraction _fileSystem;
		 private LogProvider _internalLogProvider;
		 private SystemNanoClock _clock;

		 public TestGraphDatabaseFactoryState()
		 {
			  _fileSystem = null;
			  _internalLogProvider = null;
		 }

		 public TestGraphDatabaseFactoryState( TestGraphDatabaseFactoryState previous ) : base( previous )
		 {
			  _fileSystem = previous._fileSystem;
			  _internalLogProvider = previous._internalLogProvider;
			  _clock = previous._clock;
		 }

		 public virtual FileSystemAbstraction FileSystem
		 {
			 get
			 {
				  return _fileSystem;
			 }
			 set
			 {
				  this._fileSystem = value;
			 }
		 }


		 public virtual LogProvider InternalLogProvider
		 {
			 get
			 {
				  return _internalLogProvider;
			 }
			 set
			 {
				  this._internalLogProvider = value;
			 }
		 }


		 public virtual SystemNanoClock Clock()
		 {
			  return _clock;
		 }

		 public virtual SystemNanoClock Clock
		 {
			 set
			 {
				  this._clock = value;
			 }
		 }
	}

}