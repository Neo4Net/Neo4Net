/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.impl.transaction.command
{
	using Neo4Net.Helpers.Collections;
	using BatchTransactionApplier = Neo4Net.Kernel.Impl.Api.BatchTransactionApplier;
	using TransactionApplier = Neo4Net.Kernel.Impl.Api.TransactionApplier;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using LockGroup = Neo4Net.Kernel.impl.locking.LockGroup;
	using CommandsToApply = Neo4Net.Storageengine.Api.CommandsToApply;

	/// <summary>
	/// Serves as executor of transactions, i.e. the visit... methods and will invoke the other lifecycle methods like {@link
	/// BatchTransactionApplier#startTx(CommandsToApply, LockGroup)}, <seealso cref="TransactionApplier.close()"/> ()} a.s.o
	/// correctly. Note that <seealso cref="BatchTransactionApplier.close()"/> is also called at the end.
	/// </summary>
	public class CommandHandlerContract
	{
		 private CommandHandlerContract()
		 {
		 }

		 public delegate bool ApplyFunction( TransactionApplier applier );

		 /// <summary>
		 /// Simply calls through to the <seealso cref="TransactionRepresentation.accept(Visitor)"/> method for each {@link
		 /// TransactionToApply} given. This assumes that the <seealso cref="BatchTransactionApplier"/> will return {@link
		 /// TransactionApplier}s which actually do the work and that the transaction has all the relevant data.
		 /// </summary>
		 /// <param name="applier"> to use </param>
		 /// <param name="transactions"> to apply </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void apply(org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier, org.Neo4Net.kernel.impl.api.TransactionToApply... transactions) throws Exception
		 public static void Apply( BatchTransactionApplier applier, params TransactionToApply[] transactions )
		 {
			  foreach ( TransactionToApply tx in transactions )
			  {
					using ( TransactionApplier txApplier = applier.StartTx( tx, new LockGroup() ) )
					{
						 tx.TransactionRepresentation().accept(txApplier);
					}
			  }
			  applier.Close();
		 }

		 /// <summary>
		 /// In case the transactions do not have the commands to apply, use this method to apply any commands you want with a
		 /// given <seealso cref="ApplyFunction"/> instead.
		 /// </summary>
		 /// <param name="applier"> to use </param>
		 /// <param name="function"> which knows what to do with the <seealso cref="TransactionApplier"/>. </param>
		 /// <param name="transactions"> are only used to create <seealso cref="TransactionApplier"/>s. The actual work is delegated to the
		 /// function. </param>
		 /// <returns> the boolean-and result of all function operations. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static boolean apply(org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier, ApplyFunction function, org.Neo4Net.kernel.impl.api.TransactionToApply... transactions) throws Exception
		 public static bool Apply( BatchTransactionApplier applier, ApplyFunction function, params TransactionToApply[] transactions )
		 {
			  bool result = true;
			  foreach ( TransactionToApply tx in transactions )
			  {
					using ( TransactionApplier txApplier = applier.StartTx( tx, new LockGroup() ) )
					{
						 result &= function( txApplier );
					}
			  }
			  applier.Close();
			  return result;
		 }
	}

}