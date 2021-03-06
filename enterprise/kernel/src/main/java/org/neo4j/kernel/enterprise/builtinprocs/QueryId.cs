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
namespace Org.Neo4j.Kernel.enterprise.builtinprocs
{
	using InvalidArgumentsException = Org.Neo4j.Kernel.Api.Exceptions.InvalidArgumentsException;

	public sealed class QueryId
	{
		 public const string QUERY_ID_PREFIX = "query-";
		 private readonly long _kernelQueryId;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static QueryId ofInternalId(long queryId) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
		 public static QueryId OfInternalId( long queryId )
		 {
			  return new QueryId( queryId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static QueryId fromExternalString(String queryIdText) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
		 public static QueryId FromExternalString( string queryIdText )
		 {
			  try
			  {
					if ( queryIdText.StartsWith( QUERY_ID_PREFIX, StringComparison.Ordinal ) )
					{
						 return new QueryId( long.Parse( queryIdText.Substring( QUERY_ID_PREFIX.Length ) ) );
					}
			  }
			  catch ( System.FormatException e )
			  {
					throw new InvalidArgumentsException( "Could not parse query id (expected format: query-1234)", e );
			  }

			  throw new InvalidArgumentsException( "Could not parse query id (expected format: query-1234)" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private QueryId(long kernelQueryId) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
		 private QueryId( long kernelQueryId )
		 {
			  if ( kernelQueryId <= 0 )
			  {
					throw new InvalidArgumentsException( "Negative query ids are not supported (expected format: query-1234)" );
			  }
			  this._kernelQueryId = kernelQueryId;
		 }

		 public long KernelQueryId()
		 {
			  return _kernelQueryId;
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

			  QueryId other = ( QueryId ) o;
			  return _kernelQueryId == other._kernelQueryId;
		 }

		 public override int GetHashCode()
		 {
			  return ( int )( _kernelQueryId ^ ( ( long )( ( ulong )_kernelQueryId >> 32 ) ) );
		 }

		 public override string ToString()
		 {
			  return QUERY_ID_PREFIX + _kernelQueryId;
		 }
	}

}