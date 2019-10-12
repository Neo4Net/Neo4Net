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
namespace Org.Neo4j.com
{

	using Org.Neo4j.Helpers.Collection;
	using CommittedTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;

	/// <summary>
	/// In response to a <seealso cref="Client.sendRequest(RequestType, RequestContext, Serializer, Deserializer) request"/>
	/// which contains a response value (T), and optionally some sort of side-effect,
	/// like <seealso cref="TransactionStreamResponse transaction stream"/> or <seealso cref="TransactionObligationResponse transaction obligation"/>.
	/// </summary>
	public abstract class Response<T> : AutoCloseable
	{
		 private readonly T _response;
		 private readonly StoreId _storeId;
		 private readonly ResourceReleaser _releaser;

		 public Response( T response, StoreId storeId, ResourceReleaser releaser )
		 {
			  this._storeId = storeId;
			  this._response = response;
			  this._releaser = releaser;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public T response() throws ServerFailureException
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public virtual T ResponseConflict()
		 {
			  return _response;
		 }

		 public virtual StoreId StoreId
		 {
			 get
			 {
				  return _storeId;
			 }
		 }

		 public override void Close()
		 {
			  _releaser.release();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <T> Response<T> empty()
		 public static Response<T> Empty<T>()
		 {
			  return ( Response<T> ) TransactionObligationResponse.EmptyResponse;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void accept(Handler handler) throws Exception;
		 public abstract void Accept( Handler handler );

		 /// <returns> {@code true} if this response has transactions to be applied as part of unpacking it,
		 /// otherwise {@code false}. </returns>
		 public abstract bool HasTransactionsToBeApplied();

		 /// <summary>
		 /// Handler of the transaction data part of a response. Callbacks for whether to await or apply
		 /// certain transactions.
		 /// </summary>
		 public interface Handler
		 {
			  /// <summary>
			  /// Called for responses that handle <seealso cref="TransactionObligationResponse transaction obligations"/>
			  /// after the obligation transaction id has been deserialized. </summary>
			  /// <param name="txId"> the obligation transaction id that must be fulfilled. </param>
			  /// <exception cref="IOException"> if there were any problems fulfilling that obligation. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void obligation(long txId) throws java.io.IOException;
			  void Obligation( long txId );

			  /// <returns> a <seealso cref="Visitor"/> which will <seealso cref="Visitor.visit(object) receive"/> calls about transactions. </returns>
			  Visitor<CommittedTransactionRepresentation, Exception> Transactions();
		 }
	}

}