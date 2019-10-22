using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Server.security.enterprise.auth
{
	using Caffeine = com.github.benmanes.caffeine.cache.Caffeine;
	using Ticker = com.github.benmanes.caffeine.cache.Ticker;
	using Cache = org.apache.shiro.cache.Cache;
	using CacheException = org.apache.shiro.cache.CacheException;
	using CacheManager = org.apache.shiro.cache.CacheManager;


	internal class ShiroCaffeineCache<K, V> : Cache<K, V>
	{
		 private readonly com.github.benmanes.caffeine.cache.Cache<K, V> _caffCache;

		 internal ShiroCaffeineCache( Ticker ticker, long ttl, int maxCapacity, bool useTTL ) : this( ticker, ForkJoinPool.commonPool(), ttl, maxCapacity, useTTL )
		 {
		 }

		 internal ShiroCaffeineCache( Ticker ticker, Executor maintenanceExecutor, long ttl, int maxCapacity, bool useTTL )
		 {
			  Caffeine<object, object> builder = Caffeine.newBuilder().maximumSize(maxCapacity).executor(maintenanceExecutor);
			  if ( useTTL )
			  {
					if ( ttl <= 0 )
					{
						 throw new System.ArgumentException( "TTL must be larger than zero." );
					}
					builder.ticker( ticker ).expireAfterWrite( ttl, TimeUnit.MILLISECONDS );
			  }
			  _caffCache = builder.build();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public V get(K key) throws org.apache.shiro.cache.CacheException
		 public override V Get( K key )
		 {
			  return _caffCache.getIfPresent( key );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public V put(K key, V value) throws org.apache.shiro.cache.CacheException
		 public override V Put( K key, V value )
		 {
			  return _caffCache.asMap().put(key, value);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public V remove(K key) throws org.apache.shiro.cache.CacheException
		 public override V Remove( K key )
		 {
			  return _caffCache.asMap().remove(key);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void clear() throws org.apache.shiro.cache.CacheException
		 public override void Clear()
		 {
			  _caffCache.invalidateAll();
		 }

		 public override int Size()
		 {
			  return _caffCache.asMap().size();
		 }

		 public override ISet<K> Keys()
		 {
			  return _caffCache.asMap().Keys;
		 }

		 public override ICollection<V> Values()
		 {
			  return _caffCache.asMap().values();
		 }

		 internal class Manager : CacheManager
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Map<String,org.apache.shiro.cache.Cache<?,?>> caches;
			  internal readonly IDictionary<string, Cache<object, ?>> Caches;
			  internal readonly Ticker Ticker;
			  internal readonly long Ttl;
			  internal readonly int MaxCapacity;
			  internal bool UseTTL;

			  internal Manager( Ticker ticker, long ttl, int maxCapacity, bool useTTL )
			  {
					this.Ticker = ticker;
					this.Ttl = ttl;
					this.MaxCapacity = maxCapacity;
					this.UseTTL = useTTL;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: caches = new java.util.HashMap<>();
					Caches = new Dictionary<string, Cache<object, ?>>();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <K, V> org.apache.shiro.cache.Cache<K,V> getCache(String s) throws org.apache.shiro.cache.CacheException
			  public override Cache<K, V> GetCache<K, V>( string s )
			  {
					//noinspection unchecked
					return ( Cache<K, V> ) Caches.computeIfAbsent( s, ignored => UseTTL && Ttl <= 0 ? new NullCache() : new ShiroCaffeineCache<K, V>(Ticker, Ttl, MaxCapacity, UseTTL) );
			  }
		 }

		 private class NullCache<K, V> : Cache<K, V>
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public V get(K key) throws org.apache.shiro.cache.CacheException
			  public override V Get( K key )
			  {
					return default( V );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public V put(K key, V value) throws org.apache.shiro.cache.CacheException
			  public override V Put( K key, V value )
			  {
					return default( V );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public V remove(K key) throws org.apache.shiro.cache.CacheException
			  public override V Remove( K key )
			  {
					return default( V );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void clear() throws org.apache.shiro.cache.CacheException
			  public override void Clear()
			  {

			  }

			  public override int Size()
			  {
					return 0;
			  }

			  public override ISet<K> Keys()
			  {
					return Collections.emptySet();
			  }

			  public override ICollection<V> Values()
			  {
					return Collections.emptySet();
			  }
		 }
	}

}