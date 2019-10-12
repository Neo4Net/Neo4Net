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
namespace Org.Neo4j.metrics.source.causalclustering
{
	using InFlightCacheMonitor = Org.Neo4j.causalclustering.core.consensus.log.cache.InFlightCacheMonitor;

	public class InFlightCacheMetric : InFlightCacheMonitor
	{
		 private volatile long _misses;
		 private volatile long _hits;
		 private volatile long _totalBytes;
		 private volatile long _maxBytes;
		 private volatile int _elementCount;
		 private volatile int _maxElements;

		 public override void Miss()
		 {
			  _misses++;
		 }

		 public override void Hit()
		 {
			  _hits++;
		 }

		 public virtual long Misses
		 {
			 get
			 {
				  return _misses;
			 }
		 }

		 public virtual long Hits
		 {
			 get
			 {
				  return _hits;
			 }
		 }

		 public virtual long MaxBytes
		 {
			 get
			 {
				  return _maxBytes;
			 }
			 set
			 {
				  this._maxBytes = value;
			 }
		 }

		 public virtual long TotalBytes
		 {
			 get
			 {
				  return _totalBytes;
			 }
			 set
			 {
				  this._totalBytes = value;
			 }
		 }

		 public virtual long getMaxElements()
		 {
			  return _maxElements;
		 }

		 public virtual long getElementCount()
		 {
			  return _elementCount;
		 }



		 public override void SetMaxElements( int maxElements )
		 {
			  this._maxElements = maxElements;
		 }

		 public override void SetElementCount( int elementCount )
		 {
			  this._elementCount = elementCount;
		 }
	}

}