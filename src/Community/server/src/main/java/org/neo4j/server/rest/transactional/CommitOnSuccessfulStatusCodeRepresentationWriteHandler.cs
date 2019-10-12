/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Server.rest.transactional
{
	using HttpContext = com.sun.jersey.api.core.HttpContext;
	using HttpResponseContext = com.sun.jersey.api.core.HttpResponseContext;

	using Transaction = Neo4Net.Graphdb.Transaction;
	using RepresentationWriteHandler = Neo4Net.Server.rest.repr.RepresentationWriteHandler;

	public class CommitOnSuccessfulStatusCodeRepresentationWriteHandler : RepresentationWriteHandler
	{
		 private readonly HttpContext _httpContext;
		 private Transaction _transaction;

		 public CommitOnSuccessfulStatusCodeRepresentationWriteHandler( HttpContext httpContext, Transaction transaction )
		 {
			  this._httpContext = httpContext;
			  this._transaction = transaction;
		 }

		 public override void OnRepresentationStartWriting()
		 {
			  // do nothing
		 }

		 public override void OnRepresentationWritten()
		 {
			  HttpResponseContext response = _httpContext.Response;

			  int statusCode = response.Status;
			  if ( statusCode >= 200 && statusCode < 300 )
			  {
					_transaction.success();
			  }
		 }

		 public override void OnRepresentationFinal()
		 {
			  CloseTransaction();
		 }

		 public virtual void CloseTransaction()
		 {
			  _transaction.close();
		 }

		 public virtual Transaction Transaction
		 {
			 set
			 {
				  this._transaction = value;
			 }
		 }
	}

}