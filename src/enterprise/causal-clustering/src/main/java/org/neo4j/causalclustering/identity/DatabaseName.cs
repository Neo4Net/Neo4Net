/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.identity
{

	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using Neo4Net.causalclustering.core.state.storage;
	using StringMarshal = Neo4Net.causalclustering.messaging.marshalling.StringMarshal;
	using ReadableChannel = Neo4Net.Storageengine.Api.ReadableChannel;
	using WritableChannel = Neo4Net.Storageengine.Api.WritableChannel;

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
//ORIGINAL LINE: protected DatabaseName unmarshal0(org.Neo4Net.storageengine.api.ReadableChannel channel) throws java.io.IOException
			  protected internal override DatabaseName Unmarshal0( ReadableChannel channel )
			  {
					return new DatabaseName( StringMarshal.unmarshal( channel ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(DatabaseName databaseName, org.Neo4Net.storageengine.api.WritableChannel channel) throws java.io.IOException
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