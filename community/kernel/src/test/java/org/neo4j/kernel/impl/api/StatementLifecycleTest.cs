﻿/*
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
namespace Org.Neo4j.Kernel.Impl.Api
{
	using Test = org.junit.Test;

	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using StorageReader = Org.Neo4j.Storageengine.Api.StorageReader;
	using LockTracer = Org.Neo4j.Storageengine.Api.@lock.LockTracer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;

	public class StatementLifecycleTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReleaseStoreStatementOnlyWhenReferenceCountDownToZero()
		 public virtual void ShouldReleaseStoreStatementOnlyWhenReferenceCountDownToZero()
		 {
			  // given
			  KernelTransactionImplementation transaction = mock( typeof( KernelTransactionImplementation ) );
			  StorageReader storageReader = mock( typeof( StorageReader ) );
			  KernelStatement statement = GetKernelStatement( transaction, storageReader );
			  statement.Acquire();
			  verify( storageReader ).acquire();
			  statement.Acquire();

			  // when
			  statement.Close();
			  verifyNoMoreInteractions( storageReader );

			  // then
			  statement.Close();
			  verify( storageReader ).release();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReleaseStoreStatementWhenForceClosingStatements()
		 public virtual void ShouldReleaseStoreStatementWhenForceClosingStatements()
		 {
			  // given
			  KernelTransactionImplementation transaction = mock( typeof( KernelTransactionImplementation ) );
			  StorageReader storageReader = mock( typeof( StorageReader ) );
			  KernelStatement statement = GetKernelStatement( transaction, storageReader );
			  statement.Acquire();

			  // when
			  try
			  {
					statement.ForceClose();
			  }
			  catch ( KernelStatement.StatementNotClosedException )
			  {
					//ignored
			  }

			  // then
			  verify( storageReader ).release();
		 }

		 private KernelStatement GetKernelStatement( KernelTransactionImplementation transaction, StorageReader storageReader )
		 {
			  return new KernelStatement( transaction, null, storageReader, LockTracer.NONE, mock( typeof( StatementOperationParts ) ), new ClockContext(), EmptyVersionContextSupplier.EMPTY );
		 }
	}

}