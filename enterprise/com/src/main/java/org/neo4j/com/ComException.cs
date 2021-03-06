﻿using System;

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
	using Log = Org.Neo4j.Logging.Log;

	public class ComException : Exception
	{
		 public static readonly bool TraceHaConnectivity = Boolean.getBoolean( "org.neo4j.com.TRACE_HA_CONNECTIVITY" );

		 public ComException() : base()
		 {
		 }

		 public ComException( string message, Exception cause ) : base( message, cause )
		 {
		 }

		 public ComException( string message ) : base( message )
		 {
		 }

		 public ComException( Exception cause ) : base( cause )
		 {
		 }

		 public virtual ComException TraceComException( Log log, string tracePoint )
		 {
			  if ( TraceHaConnectivity )
			  {
					string msg = string.Format( "ComException@{0:x} trace from {1}: {2}", System.identityHashCode( this ), tracePoint, Message );
					log.Debug( msg, this );
			  }
			  return this;
		 }
	}

}