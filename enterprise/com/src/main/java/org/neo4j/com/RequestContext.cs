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
namespace Org.Neo4j.com
{

	/// <summary>
	/// A representation of the context in which an HA slave operates.
	/// Contains
	/// <ul>
	/// <li>the machine id</li>
	/// <li>a list of the last applied transaction id for each datasource</li>
	/// <li>an event identifier, the txid of the most recent local top level tx</li>
	/// <li>a session id, the startup time of the database</li>
	/// </ul>
	/// </summary>
	public sealed class RequestContext
	{
		 private readonly int _machineId;
		 private readonly long _lastAppliedTransaction;
		 private readonly int _eventIdentifier;
		 private readonly int _hashCode;
		 private readonly long _epoch;
		 private readonly long _checksum;

		 public RequestContext( long epoch, int machineId, int eventIdentifier, long lastAppliedTransaction, long checksum )
		 {
			  this._epoch = epoch;
			  this._machineId = machineId;
			  this._eventIdentifier = eventIdentifier;
			  this._lastAppliedTransaction = lastAppliedTransaction;
			  this._checksum = checksum;

			  long hash = epoch;
			  hash = ( 31 * hash ) ^ eventIdentifier;
			  hash = ( 31 * hash ) ^ machineId;
			  this._hashCode = ( int )( ( ( long )( ( ulong )hash >> 32 ) ) ^ hash );
		 }

		 public int MachineId()
		 {
			  return _machineId;
		 }

		 public long LastAppliedTransaction()
		 {
			  return _lastAppliedTransaction;
		 }

		 public int EventIdentifier
		 {
			 get
			 {
				  return _eventIdentifier;
			 }
		 }

		 public long Epoch
		 {
			 get
			 {
				  return _epoch;
			 }
		 }

		 public long Checksum
		 {
			 get
			 {
				  return _checksum;
			 }
		 }

		 public override string ToString()
		 {
			  return "RequestContext[" +
						 "machineId=" + _machineId +
						 ", lastAppliedTransaction=" + _lastAppliedTransaction +
						 ", eventIdentifier=" + _eventIdentifier +
						 ", hashCode=" + _hashCode +
						 ", epoch=" + _epoch +
						 ", checksum=" + _checksum +
						 ']';
		 }

		 public override bool Equals( object obj )
		 {
			  if ( !( obj is RequestContext ) )
			  {
					return false;
			  }
			  RequestContext o = ( RequestContext ) obj;
			  return o._eventIdentifier == _eventIdentifier && o._machineId == _machineId && o._epoch == _epoch;
		 }

		 public override int GetHashCode()
		 {
			  return this._hashCode;
		 }

		 public static readonly RequestContext Empty = new RequestContext( -1, -1, -1, -1, -1 );

		 public static RequestContext Anonymous( long lastAppliedTransaction )
		 {
			  return new RequestContext( Empty._epoch, Empty._machineId, Empty._eventIdentifier, lastAppliedTransaction, Empty._checksum );
		 }
	}

}