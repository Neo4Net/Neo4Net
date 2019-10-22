using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{

	using PageCache = Neo4Net.Io.pagecache.PageCache;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Format.bytes;

	public class PageCacheArrayFactoryMonitor : NumberArrayFactory_Monitor
	{
		 // This field is designed to allow multiple threads setting it concurrently, where one of those will win and either one is fine
		 // because this monitor mostly revolves around highlighting the fact that the page cache number array is in use at all.
		 private readonly AtomicReference<string> _failedFactoriesDescription = new AtomicReference<string>();

		 public override void AllocationSuccessful( long memory, NumberArrayFactory successfulFactory, IEnumerable<NumberArrayFactory_AllocationFailure> attemptedAllocationFailures )
		 {
			  if ( successfulFactory is PageCachedNumberArrayFactory )
			  {
					StringBuilder builder = new StringBuilder( format( "Memory allocation of %s ended up in page cache, which may impact performance negatively", bytes( memory ) ) );
					attemptedAllocationFailures.forEach( failure => builder.Append( format( "%n%s: %s", failure.Factory, failure.Failure ) ) );
					_failedFactoriesDescription.compareAndSet( null, builder.ToString() );
			  }
		 }

		 /// <summary>
		 /// Called by user-facing progress monitor at arbitrary points to get information about whether or not there has been
		 /// one or more <seealso cref="NumberArrayFactory"/> allocations backed by the <seealso cref="PageCache"/>, this because it severely affects
		 /// performance. Calling this method clears the failure description, if any.
		 /// </summary>
		 /// <returns> if there have been <seealso cref="NumberArrayFactory"/> allocations backed by the <seealso cref="PageCache"/> since the last call to this method
		 /// then a description of why it was chosen is returned, otherwise {@code null}. </returns>
		 public virtual string PageCacheAllocationOrNull()
		 {
			  string failure = _failedFactoriesDescription.get();
			  if ( !string.ReferenceEquals( failure, null ) )
			  {
					_failedFactoriesDescription.compareAndSet( failure, null );
			  }
			  return failure;
		 }
	}

}