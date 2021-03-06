﻿/*
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
namespace Org.Neo4j.causalclustering.identity
{

	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using Org.Neo4j.causalclustering.core.state.storage;
	using StringMarshal = Org.Neo4j.causalclustering.messaging.marshalling.StringMarshal;
	using ReadableChannel = Org.Neo4j.Storageengine.Api.ReadableChannel;
	using WritableChannel = Org.Neo4j.Storageengine.Api.WritableChannel;

	/// <summary>
	/// Simple wrapper class for database name strings. These values are provided using the
	/// <seealso cref="CausalClusteringSettings.database "/> setting.
	/// </summary>
	public class DatabaseName
	{
		 private readonly string _name;

		 public DatabaseName( string name )
		 {
			  this._name = name;
		 }

		 public virtual string Name()
		 {
			  return _name;
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
			  DatabaseName that = ( DatabaseName ) o;
			  return Objects.Equals( _name, that._name );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _name );
		 }

		 public class Marshal : SafeStateMarshal<DatabaseName>
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected DatabaseName unmarshal0(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
			  protected internal override DatabaseName Unmarshal0( ReadableChannel channel )
			  {
					return new DatabaseName( StringMarshal.unmarshal( channel ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(DatabaseName databaseName, org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  public override void MarshalConflict( DatabaseName databaseName, WritableChannel channel )
			  {
					StringMarshal.marshal( channel, databaseName.Name() );
			  }

			  public override DatabaseName StartState()
			  {
					return null;
			  }

			  public override long Ordinal( DatabaseName databaseName )
			  {
					return databaseName == null ? 0 : 1;
			  }
		 }
	}

}