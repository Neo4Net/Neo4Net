using System;
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
namespace Neo4Net.Harness.Internal
{

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using FileUtils = Neo4Net.Io.fs.FileUtils;

	/// <summary>
	/// Manages user-defined cypher fixtures that can be exercised against the server.
	/// </summary>
	public class Fixtures
	{
		 private readonly IList<string> _fixtureStatements = new LinkedList<string>();
		 private readonly IList<System.Func<GraphDatabaseService, Void>> _fixtureFunctions = new LinkedList<System.Func<GraphDatabaseService, Void>>();

		 private readonly string _cypherSuffix = "cyp";

		 private readonly FileFilter _cypherFileOrDirectoryFilter = file =>
		 {
		  if ( file.Directory )
		  {
				return true;
		  }
		  string[] split = file.Name.Split( "\\." );
		  string suffix = split[split.Length - 1];
		  return suffix.Equals( _cypherSuffix );
		 };

		 public virtual void Add( File fixturePath )
		 {
			  try
			  {
					if ( fixturePath.Directory )
					{
						 foreach ( File file in fixturePath.listFiles( _cypherFileOrDirectoryFilter ) )
						 {
							  Add( file );
						 }
						 return;
					}
					add( FileUtils.readTextFile( fixturePath, StandardCharsets.UTF_8 ) );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( "Unable to read fixture file '" + fixturePath.AbsolutePath + "': " + e.Message, e );
			  }
		 }

		 public virtual void Add( string statement )
		 {
			  if ( statement.Trim().Length > 0 )
			  {
					_fixtureStatements.Add( statement );
			  }
		 }

		 public virtual void Add( System.Func<GraphDatabaseService, Void> fixtureFunction )
		 {
			  _fixtureFunctions.Add( fixtureFunction );
		 }

		 public virtual void ApplyTo( InProcessServerControls controls )
		 {
			  GraphDatabaseService db = controls.Graph();
			  foreach ( string fixtureStatement in _fixtureStatements )
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.execute( fixtureStatement );
						 tx.Success();
					}
			  }
			  foreach ( System.Func<GraphDatabaseService, Void> fixtureFunction in _fixtureFunctions )
			  {
					fixtureFunction( db );
			  }
		 }
	}

}