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
namespace Neo4Net.com
{
	using ResponseUnpacker = Neo4Net.com.storecopy.ResponseUnpacker;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;

	/// <summary>
	/// <seealso cref="Response"/> that carries <seealso cref="TransactionStream transaction data"/> as a side-effect, to be applied
	/// before accessing the response value.
	/// </summary>
	/// <seealso cref= ResponseUnpacker </seealso>
	public class TransactionStreamResponse<T> : Response<T>
	{
		 private readonly TransactionStream _transactions;

		 public TransactionStreamResponse( T response, StoreId storeId, TransactionStream transactions, ResourceReleaser releaser ) : base( response, storeId, releaser )
		 {
			  this._transactions = transactions;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void accept(Response.Handler handler) throws Exception
		 public override void Accept( Response.Handler handler )
		 {
			  _transactions.accept( handler.Transactions() );
		 }

		 public override bool HasTransactionsToBeApplied()
		 {
			  return true;
		 }
	}

}