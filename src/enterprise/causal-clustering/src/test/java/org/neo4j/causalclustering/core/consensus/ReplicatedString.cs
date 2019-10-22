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
namespace Neo4Net.causalclustering.core.consensus
{
	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;
	using ReplicatedContentHandler = Neo4Net.causalclustering.messaging.marshalling.ReplicatedContentHandler;

	public class ReplicatedString : ReplicatedContent
	{
		 private readonly string _value;

		 public ReplicatedString( string data )
		 {
			  this._value = data;
		 }

		 public static ReplicatedString ValueOf( string value )
		 {
			  return new ReplicatedString( value );
		 }

		 public virtual string Get()
		 {
			  return _value;
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

			  ReplicatedString that = ( ReplicatedString ) o;
			  return _value.Equals( that._value );
		 }

		 public override int GetHashCode()
		 {
			  return _value.GetHashCode();
		 }

		 public override string ToString()
		 {
			  return format( "ReplicatedString{data=%s}", _value );
		 }

		 public virtual string Value()
		 {
			  return _value;
		 }

		 public override void Handle( ReplicatedContentHandler contentHandler )
		 {
			  throw new System.NotSupportedException( "No handler for this " + this.GetType() );
		 }
	}

}