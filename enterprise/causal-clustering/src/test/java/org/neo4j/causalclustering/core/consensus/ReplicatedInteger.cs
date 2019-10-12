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
namespace Org.Neo4j.causalclustering.core.consensus
{

	using ReplicatedContent = Org.Neo4j.causalclustering.core.replication.ReplicatedContent;
	using ReplicatedContentHandler = Org.Neo4j.causalclustering.messaging.marshalling.ReplicatedContentHandler;

	public class ReplicatedInteger : ReplicatedContent
	{
		 private readonly int? _value;

		 private ReplicatedInteger( int? data )
		 {
			  Objects.requireNonNull( data );
			  this._value = data;
		 }

		 public static ReplicatedInteger ValueOf( int? value )
		 {
			  return new ReplicatedInteger( value );
		 }

		 public virtual int Get()
		 {
			  return _value.Value;
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

			  ReplicatedInteger that = ( ReplicatedInteger ) o;
			  return _value.Equals( that._value );
		 }

		 public override int GetHashCode()
		 {
			  return _value.GetHashCode();
		 }

		 public override string ToString()
		 {
			  return format( "Integer(%d)", _value );
		 }

		 public override void Handle( ReplicatedContentHandler contentHandler )
		 {
			  throw new System.NotSupportedException( "No handler for this " + this.GetType() );
		 }
	}

}