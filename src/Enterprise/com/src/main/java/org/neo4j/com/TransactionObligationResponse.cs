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

	using TransactionObligationFulfiller = Neo4Net.com.storecopy.TransactionObligationFulfiller;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;

	/// <summary>
	/// <seealso cref="Response"/> that carries transaction obligation as a side-effect.
	/// </summary>
	/// <seealso cref= TransactionObligationFulfiller </seealso>
	public class TransactionObligationResponse<T> : Response<T>
	{
		 internal const sbyte RESPONSE_TYPE = -1;

		 internal static readonly Response<Void> EmptyResponse = new TransactionObligationResponse<Void>( null, StoreId.DEFAULT, -1, ResourceReleaser_Fields.NoOp );

		 private readonly long _obligationTxId;

		 public TransactionObligationResponse( T response, StoreId storeId, long obligationTxId, ResourceReleaser releaser ) : base( response, storeId, releaser )
		 {
			  this._obligationTxId = obligationTxId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void accept(Response.Handler handler) throws java.io.IOException
		 public override void Accept( Response.Handler handler )
		 {
			  handler.Obligation( _obligationTxId );
		 }

		 public override bool HasTransactionsToBeApplied()
		 {
			  return false;
		 }
	}

}