using System.Diagnostics;

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
namespace Neo4Net.Kernel.ha
{
	using Neo4Net.com;
	using RequestType = Neo4Net.com.RequestType;
	using Neo4Net.com;

	internal abstract class AbstractHaRequestTypes : HaRequestTypes
	{
		 private readonly HaRequestType[] _types = new HaRequestType[HaRequestTypes_Type.values().length];

		 protected internal virtual void Register<A, B, C>( HaRequestTypes_Type type, TargetCaller<A, B> targetCaller, ObjectSerializer<C> objectSerializer )
		 {
			  Register( type, targetCaller, objectSerializer, true );
		 }

		 protected internal virtual void Register<A, B, C>( HaRequestTypes_Type type, TargetCaller<A, B> targetCaller, ObjectSerializer<C> objectSerializer, bool unpack )
		 {
			  Debug.Assert( _types[type.ordinal()] == null );
			  _types[type.ordinal()] = new HaRequestType(targetCaller, objectSerializer, (sbyte)type.ordinal(), unpack);
		 }

		 public override RequestType Type( HaRequestTypes_Type type )
		 {
			  return type( ( sbyte ) type.ordinal() );
		 }

		 public override RequestType Type( sbyte id )
		 {
			  HaRequestType requestType = _types[id];
			  if ( requestType == null )
			  {
					throw new System.NotSupportedException( "Not used anymore, merely here to keep the ordinal ids of the others" );
			  }
			  return requestType;
		 }
	}

}