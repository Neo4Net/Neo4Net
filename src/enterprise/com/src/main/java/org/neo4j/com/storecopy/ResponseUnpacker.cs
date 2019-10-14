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
namespace Neo4Net.com.storecopy
{
	using Neo4Net.com;

	public interface ResponseUnpacker
	{
		 /// <param name="txHandler"> for getting an insight into which transactions gets applied. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void unpackResponse(org.neo4j.com.Response<?> response, ResponseUnpacker_TxHandler txHandler) throws Exception;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 void unpackResponse<T1>( Response<T1> response, ResponseUnpacker_TxHandler txHandler );
	}

	public static class ResponseUnpacker_Fields
	{
		 public static readonly ResponseUnpacker NoOpResponseUnpacker = ( response, txHandler ) =>
		 {
		  /* Do nothing */
		 };

	}

	 public interface ResponseUnpacker_TxHandler
	 {

		  void Accept( long transactionId );
	 }

	 public static class ResponseUnpacker_TxHandler_Fields
	 {
		  public static readonly ResponseUnpacker_TxHandler NoOpTxHandler = transactionId =>
		  {
			/* Do nothing */
		  };

	 }

}