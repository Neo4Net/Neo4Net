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
namespace Neo4Net.Kernel
{
	using KernelImpl = Neo4Net.Kernel.Impl.Api.KernelImpl;
	using KernelTransactions = Neo4Net.Kernel.Impl.Api.KernelTransactions;
	using TransactionCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionCommitProcess;
	using NeoStoreFileListing = Neo4Net.Kernel.impl.transaction.state.NeoStoreFileListing;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;

	internal class NeoStoreKernelModule
	{
		 private readonly TransactionCommitProcess _transactionCommitProcess;
		 private readonly KernelImpl _kernel;
		 private readonly KernelTransactions _kernelTransactions;
		 private readonly NeoStoreFileListing _fileListing;

		 internal NeoStoreKernelModule( TransactionCommitProcess transactionCommitProcess, KernelImpl kernel, KernelTransactions kernelTransactions, NeoStoreFileListing fileListing )
		 {
			  this._transactionCommitProcess = transactionCommitProcess;
			  this._kernel = kernel;
			  this._kernelTransactions = kernelTransactions;
			  this._fileListing = fileListing;
		 }

		 public virtual KernelImpl KernelAPI()
		 {
			  return _kernel;
		 }

		 internal virtual KernelTransactions KernelTransactions()
		 {
			  return _kernelTransactions;
		 }

		 internal virtual NeoStoreFileListing FileListing()
		 {
			  return _fileListing;
		 }

		 public virtual void SatisfyDependencies( Dependencies dependencies )
		 {
			  dependencies.SatisfyDependencies( _transactionCommitProcess, _kernel, _kernelTransactions, _fileListing );
		 }
	}

}