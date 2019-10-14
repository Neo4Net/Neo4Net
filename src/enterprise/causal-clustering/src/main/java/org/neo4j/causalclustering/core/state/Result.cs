using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.core.state
{

	public class Result
	{
		 private readonly Exception _exception;
		 private readonly object _result;

		 private Result( Exception exception )
		 {
			  this._exception = exception;
			  this._result = null;
		 }

		 private Result( object result )
		 {
			  this._result = result;
			  this._exception = null;
		 }

		 public static Result Of( object result )
		 {
			  return new Result( result );
		 }

		 public static Result Of( Exception exception )
		 {
			  return new Result( exception );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object consume() throws Exception
		 public virtual object Consume()
		 {
			  if ( _exception != null )
			  {
					throw _exception;
			  }
			  else
			  {
					return _result;
			  }
		 }

		 public virtual CompletableFuture<object> Apply( CompletableFuture<object> future )
		 {
			  if ( _exception != null )
			  {
					future.completeExceptionally( _exception );
			  }
			  else
			  {
					future.complete( _result );
			  }

			  return future;
		 }

		 public override string ToString()
		 {
			  return "Result{" +
						"exception=" + _exception +
						", result=" + _result +
						'}';
		 }
	}

}