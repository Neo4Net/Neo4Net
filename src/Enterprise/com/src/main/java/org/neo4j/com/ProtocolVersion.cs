using System;

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
namespace Neo4Net.com
{
	public sealed class ProtocolVersion : IComparable<ProtocolVersion>
	{
		 public const sbyte INTERNAL_PROTOCOL_VERSION = 2;

		 private readonly sbyte _applicationProtocol;
		 private readonly sbyte _internalProtocol;

		 public ProtocolVersion( sbyte applicationProtocol, sbyte internalProtocol )
		 {
			  this._applicationProtocol = applicationProtocol;
			  this._internalProtocol = internalProtocol;
		 }

		 public sbyte ApplicationProtocol
		 {
			 get
			 {
				  return _applicationProtocol;
			 }
		 }

		 public sbyte InternalProtocol
		 {
			 get
			 {
				  return _internalProtocol;
			 }
		 }

		 public override bool Equals( object obj )
		 {
			  if ( obj == null )
			  {
					return false;
			  }
			  if ( obj.GetType() != typeof(ProtocolVersion) )
			  {
					return false;
			  }
			  ProtocolVersion other = ( ProtocolVersion ) obj;
			  return ( other._applicationProtocol == _applicationProtocol ) && ( other._internalProtocol == _internalProtocol );
		 }

		 public override int GetHashCode()
		 {
			  return ( 31 * _applicationProtocol ) | _internalProtocol;
		 }

		 public override int CompareTo( ProtocolVersion that )
		 {
			  return Byte.compare( this._applicationProtocol, that._applicationProtocol );
		 }

		 public override string ToString()
		 {
			  return "ProtocolVersion{" +
						 "applicationProtocol=" + _applicationProtocol +
						 ", internalProtocol=" + _internalProtocol +
						 '}';
		 }
	}

}