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
namespace Neo4Net.Bolt.v3.messaging.request
{

	using BoltIOException = Neo4Net.Bolt.messaging.BoltIOException;
	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using Bookmark = Neo4Net.Bolt.v1.runtime.bookmarking.Bookmark;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v3.messaging.request.MessageMetadataParser.parseTransactionMetadata;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v3.messaging.request.MessageMetadataParser.parseTransactionTimeout;

	public class BeginMessage : RequestMessage
	{
		 public const sbyte SIGNATURE = 0x11;

		 private readonly MapValue _meta;
		 private readonly Bookmark _bookmark;
		 private readonly Duration _txTimeout;
		 private readonly IDictionary<string, object> _txMetadata;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public BeginMessage() throws org.neo4j.bolt.messaging.BoltIOException
		 public BeginMessage() : this(VirtualValues.EMPTY_MAP)
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public BeginMessage(org.neo4j.values.virtual.MapValue meta) throws org.neo4j.bolt.messaging.BoltIOException
		 public BeginMessage( MapValue meta )
		 {
			  this._meta = requireNonNull( meta );
			  this._bookmark = Bookmark.fromParamsOrNull( meta );
			  this._txTimeout = parseTransactionTimeout( meta );
			  this._txMetadata = parseTransactionMetadata( meta );
		 }

		 public virtual Bookmark Bookmark()
		 {
			  return this._bookmark;
		 }

		 public virtual Duration TransactionTimeout()
		 {
			  return this._txTimeout;
		 }

		 public override bool SafeToProcessInAnyState()
		 {
			  return false;
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
			  BeginMessage that = ( BeginMessage ) o;
			  return Objects.Equals( _meta, that._meta );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _meta );
		 }

		 public override string ToString()
		 {
			  return "BEGIN " + _meta;
		 }

		 public virtual MapValue Meta()
		 {
			  return _meta;
		 }

		 public virtual IDictionary<string, object> TransactionMetadata()
		 {
			  return _txMetadata;
		 }
	}

}