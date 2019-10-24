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
namespace Neo4Net.Kernel.Api.Impl.Schema.sampler
{
	using TaskControl = Neo4Net.Helpers.TaskControl;
	using TaskCoordinator = Neo4Net.Helpers.TaskCoordinator;
	using IndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException;
	using IndexSampler = Neo4Net.Kernel.Api.StorageEngine.schema.IndexSampler;

	/// <summary>
	/// Abstract implementation of a Lucene index sampler, that can react on sampling being canceled via
	/// <seealso cref="TaskCoordinator.cancel()"/> }.
	/// </summary>
	internal abstract class LuceneIndexSampler : IndexSampler
	{
		public abstract IndexSample SampleIndex();
		 private readonly TaskControl _executionTicket;

		 internal LuceneIndexSampler( TaskControl taskControl )
		 {
			  this._executionTicket = taskControl;
		 }

		 /// <summary>
		 /// Check if sampling was canceled.
		 /// </summary>
		 /// <exception cref="IndexNotFoundKernelException"> if cancellation was requested. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void checkCancellation() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException
		 internal virtual void CheckCancellation()
		 {
			  if ( _executionTicket.cancellationRequested() )
			  {
					throw new IndexNotFoundKernelException( "Index dropped while sampling." );
			  }
		 }

		 public override void Close()
		 {
			  _executionTicket.close();
		 }
	}

}