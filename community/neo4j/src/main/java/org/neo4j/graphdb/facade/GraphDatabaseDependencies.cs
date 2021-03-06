﻿using System;
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
namespace Org.Neo4j.Graphdb.facade
{
	using ImmutableList = org.eclipse.collections.api.list.ImmutableList;
	using ImmutableMap = org.eclipse.collections.api.map.ImmutableMap;
	using ImmutableListFactoryImpl = org.eclipse.collections.impl.list.immutable.ImmutableListFactoryImpl;
	using ImmutableMapFactoryImpl = org.eclipse.collections.impl.map.immutable.ImmutableMapFactoryImpl;


	using URLAccessRule = Org.Neo4j.Graphdb.security.URLAccessRule;
	using Service = Org.Neo4j.Helpers.Service;
	using Org.Neo4j.Helpers.Collection;
	using Org.Neo4j.Kernel.extension;
	using QueryEngineProvider = Org.Neo4j.Kernel.impl.query.QueryEngineProvider;
	using URLAccessRules = Org.Neo4j.Kernel.impl.security.URLAccessRules;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using DeferredExecutor = Org.Neo4j.Scheduler.DeferredExecutor;
	using Group = Org.Neo4j.Scheduler.Group;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.concat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asImmutableList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asImmutableMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asIterable;

	public class GraphDatabaseDependencies : GraphDatabaseFacadeFactory.Dependencies
	{
		 public static GraphDatabaseDependencies NewDependencies( GraphDatabaseFacadeFactory.Dependencies deps )
		 {
			  return new GraphDatabaseDependencies( deps.Monitors(), deps.UserLogProvider(), asImmutableList(deps.SettingsClasses()), asImmutableList(deps.KernelExtensions()), asImmutableMap(deps.UrlAccessRules()), asImmutableList(deps.ExecutionEngines()), asImmutableList(deps.DeferredExecutors()) );
		 }

		 public static GraphDatabaseDependencies NewDependencies()
		 {
			  ImmutableList<Type> settingsClasses = ImmutableListFactoryImpl.INSTANCE.empty();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.eclipse.collections.api.list.ImmutableList<org.neo4j.kernel.extension.KernelExtensionFactory<?>> kernelExtensions = asImmutableList(getKernelExtensions(org.neo4j.helpers.Service.load(org.neo4j.kernel.extension.KernelExtensionFactory.class).iterator()));
			  ImmutableList<KernelExtensionFactory<object>> kernelExtensions = asImmutableList( GetKernelExtensions( Service.load( typeof( KernelExtensionFactory ) ).GetEnumerator() ) );

			  ImmutableMap<string, URLAccessRule> urlAccessRules = ImmutableMapFactoryImpl.INSTANCE.of( "http", URLAccessRules.alwaysPermitted(), "https", URLAccessRules.alwaysPermitted(), "ftp", URLAccessRules.alwaysPermitted(), "file", URLAccessRules.fileAccess() );

			  ImmutableList<QueryEngineProvider> queryEngineProviders = asImmutableList( Service.load( typeof( QueryEngineProvider ) ) );
			  ImmutableList<Pair<DeferredExecutor, Group>> deferredExecutors = ImmutableListFactoryImpl.INSTANCE.empty();

			  return new GraphDatabaseDependencies( null, null, settingsClasses, kernelExtensions, urlAccessRules, queryEngineProviders, deferredExecutors );
		 }

		 private readonly Monitors _monitors;
		 private readonly LogProvider _userLogProvider;
		 private readonly ImmutableList<Type> _settingsClasses;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.eclipse.collections.api.list.ImmutableList<org.neo4j.kernel.extension.KernelExtensionFactory<?>> kernelExtensions;
		 private readonly ImmutableList<KernelExtensionFactory<object>> _kernelExtensions;
		 private readonly ImmutableMap<string, URLAccessRule> _urlAccessRules;
		 private readonly ImmutableList<QueryEngineProvider> _queryEngineProviders;
		 private readonly ImmutableList<Pair<DeferredExecutor, Group>> _deferredExecutors;

		 private GraphDatabaseDependencies<T1>( Monitors monitors, LogProvider userLogProvider, ImmutableList<Type> settingsClasses, ImmutableList<T1> kernelExtensions, ImmutableMap<string, URLAccessRule> urlAccessRules, ImmutableList<QueryEngineProvider> queryEngineProviders, ImmutableList<Pair<DeferredExecutor, Group>> deferredExecutors )
		 {
			  this._monitors = monitors;
			  this._userLogProvider = userLogProvider;
			  this._settingsClasses = settingsClasses;
			  this._kernelExtensions = kernelExtensions;
			  this._urlAccessRules = urlAccessRules;
			  this._queryEngineProviders = queryEngineProviders;
			  this._deferredExecutors = deferredExecutors;
		 }

		 // Builder DSL
		 public virtual GraphDatabaseDependencies Monitors( Monitors monitors )
		 {
			  return new GraphDatabaseDependencies( monitors, _userLogProvider, _settingsClasses, _kernelExtensions, _urlAccessRules, _queryEngineProviders, _deferredExecutors );
		 }

		 public virtual GraphDatabaseDependencies UserLogProvider( LogProvider userLogProvider )
		 {
			  return new GraphDatabaseDependencies( _monitors, userLogProvider, _settingsClasses, _kernelExtensions, _urlAccessRules, _queryEngineProviders, _deferredExecutors );
		 }

		 public virtual GraphDatabaseDependencies WithDeferredExecutor( DeferredExecutor executor, Group group )
		 {
			  return new GraphDatabaseDependencies( _monitors, _userLogProvider, _settingsClasses, _kernelExtensions, _urlAccessRules, _queryEngineProviders, asImmutableList( concat( _deferredExecutors, asIterable( Pair.of( executor, group ) ) ) ) );
		 }

		 public virtual GraphDatabaseDependencies SettingsClasses( IList<Type> settingsClasses )
		 {
			  return new GraphDatabaseDependencies( _monitors, _userLogProvider, asImmutableList( settingsClasses ), _kernelExtensions, _urlAccessRules, _queryEngineProviders, _deferredExecutors );
		 }

		 public virtual GraphDatabaseDependencies SettingsClasses( params Type[] settingsClass )
		 {
			  return new GraphDatabaseDependencies( _monitors, _userLogProvider, asImmutableList( concat( _settingsClasses, Arrays.asList( settingsClass ) ) ), _kernelExtensions, _urlAccessRules, _queryEngineProviders, _deferredExecutors );
		 }

		 public virtual GraphDatabaseDependencies KernelExtensions<T1>( IEnumerable<T1> kernelExtensions )
		 {
			  return new GraphDatabaseDependencies( _monitors, _userLogProvider, _settingsClasses, asImmutableList( kernelExtensions ), _urlAccessRules, _queryEngineProviders, _deferredExecutors );
		 }

		 public virtual GraphDatabaseDependencies UrlAccessRules( IDictionary<string, URLAccessRule> urlAccessRules )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<String,org.neo4j.graphdb.security.URLAccessRule> newUrlAccessRules = this.urlAccessRules.toMap();
			  IDictionary<string, URLAccessRule> newUrlAccessRules = this._urlAccessRules.toMap();
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  newUrlAccessRules.putAll( urlAccessRules );
			  return new GraphDatabaseDependencies( _monitors, _userLogProvider, _settingsClasses, _kernelExtensions, asImmutableMap( newUrlAccessRules ), _queryEngineProviders, _deferredExecutors );
		 }

		 public virtual GraphDatabaseDependencies QueryEngineProviders( IEnumerable<QueryEngineProvider> queryEngineProviders )
		 {
			  return new GraphDatabaseDependencies( _monitors, _userLogProvider, _settingsClasses, _kernelExtensions, _urlAccessRules, asImmutableList( concat( this._queryEngineProviders, queryEngineProviders ) ), _deferredExecutors );
		 }

		 // Dependencies implementation
		 public override Monitors Monitors()
		 {
			  return _monitors;
		 }

		 public override LogProvider UserLogProvider()
		 {
			  return _userLogProvider;
		 }

		 public override IEnumerable<Type> SettingsClasses()
		 {
			  return _settingsClasses;
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public Iterable<org.neo4j.kernel.extension.KernelExtensionFactory<?>> kernelExtensions()
		 public override IEnumerable<KernelExtensionFactory<object>> KernelExtensions()
		 {
			  return _kernelExtensions;
		 }

		 public override IDictionary<string, URLAccessRule> UrlAccessRules()
		 {
			  return _urlAccessRules.castToMap();
		 }

		 public override IEnumerable<QueryEngineProvider> ExecutionEngines()
		 {
			  return _queryEngineProviders;
		 }

		 public override IEnumerable<Pair<DeferredExecutor, Group>> DeferredExecutors()
		 {
			  return _deferredExecutors;
		 }

		 // This method is needed to convert the non generic KernelExtensionFactory type returned from Service.load
		 // to KernelExtensionFactory<?> generic types
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private static java.util.Iterator<org.neo4j.kernel.extension.KernelExtensionFactory<?>> getKernelExtensions(java.util.Iterator<org.neo4j.kernel.extension.KernelExtensionFactory> parent)
		 private static IEnumerator<KernelExtensionFactory<object>> GetKernelExtensions( IEnumerator<KernelExtensionFactory> parent )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return new java.util.Iterator<org.neo4j.kernel.extension.KernelExtensionFactory<?>>()
			  return new IteratorAnonymousInnerClass( parent );
		 }

		 private class IteratorAnonymousInnerClass : IEnumerator<KernelExtensionFactory<JavaToDotNetGenericWildcard>>
		 {
			 private IEnumerator<KernelExtensionFactory> _parent;

			 public IteratorAnonymousInnerClass( IEnumerator<KernelExtensionFactory> parent )
			 {
				 this._parent = parent;
			 }

			 public bool hasNext()
			 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  return _parent.hasNext();
			 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public org.neo4j.kernel.extension.KernelExtensionFactory<?> next()
			 public KernelExtensionFactory<object> next()
			 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  return _parent.next();
			 }
		 }
	}

}