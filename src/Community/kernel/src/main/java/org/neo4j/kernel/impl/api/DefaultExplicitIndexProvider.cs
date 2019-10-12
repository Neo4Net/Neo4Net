using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Api
{

	using IndexImplementation = Neo4Net.Kernel.spi.explicitindex.IndexImplementation;
	using IndexProviders = Neo4Net.Kernel.spi.explicitindex.IndexProviders;

	public class DefaultExplicitIndexProvider : IndexProviders, ExplicitIndexProvider
	{
		 private readonly IDictionary<string, IndexImplementation> _indexProviders = new Dictionary<string, IndexImplementation>();

		 public override void RegisterIndexProvider( string name, IndexImplementation index )
		 {
			  if ( _indexProviders.ContainsKey( name ) )
			  {
					throw new System.ArgumentException( "Index provider '" + name + "' already registered" );
			  }
			  _indexProviders[name] = index;
		 }

		 public override bool UnregisterIndexProvider( string name )
		 {
			  IndexImplementation removed = _indexProviders.Remove( name );
			  return removed != null;
		 }

		 public override IndexImplementation GetProviderByName( string name )
		 {
			  IndexImplementation provider = _indexProviders[name];
			  if ( provider == null )
			  {
					throw new System.ArgumentException( "No index provider '" + name + "' found. Maybe the intended provider (or one more of its " + "dependencies) " + "aren't on the classpath or it failed to load." );
			  }
			  return provider;
		 }

		 public override IEnumerable<IndexImplementation> AllIndexProviders()
		 {
			  return _indexProviders.Values;
		 }
	}

}