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
namespace Neo4Net.management.impl
{

	using PageCacheCounters = Neo4Net.Io.pagecache.monitoring.PageCacheCounters;
	using ManagementBeanProvider = Neo4Net.Jmx.impl.ManagementBeanProvider;
	using ManagementData = Neo4Net.Jmx.impl.ManagementData;
	using Neo4jMBean = Neo4Net.Jmx.impl.Neo4jMBean;

	public sealed class PageCacheBean : ManagementBeanProvider
	{
		 public PageCacheBean() : base(typeof(PageCache))
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.jmx.impl.Neo4jMBean createMBean(org.neo4j.jmx.impl.ManagementData management) throws javax.management.NotCompliantMBeanException
		 protected internal override Neo4jMBean CreateMBean( ManagementData management )
		 {
			  return new PageCacheImpl( management );
		 }

		 private class PageCacheImpl : Neo4jMBean, PageCache
		 {
			  internal readonly PageCacheCounters PageCacheCounters;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PageCacheImpl(org.neo4j.jmx.impl.ManagementData management) throws javax.management.NotCompliantMBeanException
			  internal PageCacheImpl( ManagementData management ) : base( management )
			  {
					this.PageCacheCounters = management.ResolveDependency( typeof( PageCacheCounters ) );
			  }

			  public virtual long Faults
			  {
				  get
				  {
						return PageCacheCounters.faults();
				  }
			  }

			  public virtual long Evictions
			  {
				  get
				  {
						return PageCacheCounters.evictions();
				  }
			  }

			  public virtual long Pins
			  {
				  get
				  {
						return PageCacheCounters.pins();
				  }
			  }

			  public virtual long Unpins
			  {
				  get
				  {
						return PageCacheCounters.unpins();
				  }
			  }

			  public virtual long Hits
			  {
				  get
				  {
						return PageCacheCounters.hits();
				  }
			  }

			  public virtual long Flushes
			  {
				  get
				  {
						return PageCacheCounters.flushes();
				  }
			  }

			  public virtual long BytesRead
			  {
				  get
				  {
						return PageCacheCounters.bytesRead();
				  }
			  }

			  public virtual long BytesWritten
			  {
				  get
				  {
						return PageCacheCounters.bytesWritten();
				  }
			  }

			  public virtual long FileMappings
			  {
				  get
				  {
						return PageCacheCounters.filesMapped();
				  }
			  }

			  public virtual long FileUnmappings
			  {
				  get
				  {
						return PageCacheCounters.filesUnmapped();
				  }
			  }

			  public virtual double HitRatio
			  {
				  get
				  {
						return PageCacheCounters.hitRatio();
				  }
			  }

			  public virtual long EvictionExceptions
			  {
				  get
				  {
						return PageCacheCounters.evictionExceptions();
				  }
			  }

			  public virtual double UsageRatio
			  {
				  get
				  {
						return PageCacheCounters.usageRatio();
				  }
			  }
		 }
	}

}