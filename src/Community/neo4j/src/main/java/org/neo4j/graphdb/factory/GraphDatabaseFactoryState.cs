using System;
using System.Collections.Concurrent;
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
namespace Neo4Net.Graphdb.factory
{

	using LoadableConfig = Neo4Net.Configuration.LoadableConfig;
	using GraphDatabaseFacadeFactory = Neo4Net.Graphdb.facade.GraphDatabaseFacadeFactory;
	using URLAccessRule = Neo4Net.Graphdb.security.URLAccessRule;
	using Service = Neo4Net.Helpers.Service;
	using Neo4Net.Kernel.extension;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.facade.GraphDatabaseDependencies.newDependencies;

	/// @deprecated This will be moved to an internal package in the future. 
	[Obsolete("This will be moved to an internal package in the future.")]
	public class GraphDatabaseFactoryState
	{
		 // Keep these fields volatile or equivalent because of this scenario:
		 // - one thread creates a GraphDatabaseFactory (including state)
		 // - this factory will potentially be handed over to other threads, which will create databases
		 private readonly IList<Type> _settingsClasses;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.List<org.neo4j.kernel.extension.KernelExtensionFactory<?>> kernelExtensions;
		 private readonly IList<KernelExtensionFactory<object>> _kernelExtensions;
		 private volatile Monitors _monitors;
		 private volatile LogProvider _userLogProvider;
		 private readonly IDictionary<string, URLAccessRule> _urlAccessRules;

		 public GraphDatabaseFactoryState()
		 {
			  _settingsClasses = new CopyOnWriteArrayList<Type>();
			  _settingsClasses.Add( typeof( GraphDatabaseSettings ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: kernelExtensions = new java.util.concurrent.CopyOnWriteArrayList<>();
			  _kernelExtensions = new CopyOnWriteArrayList<KernelExtensionFactory<object>>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.neo4j.kernel.extension.KernelExtensionFactory<?> factory : org.neo4j.helpers.Service.load(org.neo4j.kernel.extension.KernelExtensionFactory.class))
			  foreach ( KernelExtensionFactory<object> factory in Service.load( typeof( KernelExtensionFactory ) ) )
			  {
					_kernelExtensions.Add( factory );
			  }
			  _urlAccessRules = new ConcurrentDictionary<string, URLAccessRule>();
		 }

		 public GraphDatabaseFactoryState( GraphDatabaseFactoryState previous )
		 {
			  _settingsClasses = new CopyOnWriteArrayList<Type>( previous._settingsClasses );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: kernelExtensions = new java.util.concurrent.CopyOnWriteArrayList<>(previous.kernelExtensions);
			  _kernelExtensions = new CopyOnWriteArrayList<KernelExtensionFactory<object>>( previous._kernelExtensions );
			  _urlAccessRules = new ConcurrentDictionary<string, URLAccessRule>( previous._urlAccessRules );
			  _monitors = previous._monitors;
			  _userLogProvider = previous._userLogProvider;
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public Iterable<org.neo4j.kernel.extension.KernelExtensionFactory<?>> getKernelExtension()
		 public virtual IEnumerable<KernelExtensionFactory<object>> KernelExtension
		 {
			 get
			 {
				  return _kernelExtensions;
			 }
		 }

		 public virtual void RemoveKernelExtensions<T1>( System.Predicate<T1> toRemove )
		 {
			  _kernelExtensions.removeIf( toRemove );
		 }

		 public virtual IEnumerable<T1> KernelExtensions<T1>
		 {
			 set
			 {
				  _kernelExtensions.Clear();
				  AddKernelExtensions( value );
			 }
		 }

		 public virtual void AddKernelExtensions<T1>( IEnumerable<T1> newKernelExtensions )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.neo4j.kernel.extension.KernelExtensionFactory<?> newKernelExtension : newKernelExtensions)
			  foreach ( KernelExtensionFactory<object> newKernelExtension in newKernelExtensions )
			  {
					_kernelExtensions.Add( newKernelExtension );
			  }
		 }

		 /// <param name="settings"> a class with all settings. </param>
		 /// @deprecated This method has no side effects now since we moved to service loading instead, <seealso cref="LoadableConfig"/>
		 /// should be used. 
		 [Obsolete("This method has no side effects now since we moved to service loading instead, <seealso cref=\"LoadableConfig\"/>")]
		 public virtual void AddSettingsClasses( IEnumerable<Type> settings )
		 {
			  foreach ( Type setting in settings )
			  {
					_settingsClasses.Add( setting );
			  }
		 }

		 public virtual void AddURLAccessRule( string protocol, URLAccessRule rule )
		 {
			  _urlAccessRules[protocol] = rule;
		 }

		 public virtual LogProvider UserLogProvider
		 {
			 set
			 {
				  this._userLogProvider = value;
			 }
		 }

		 public virtual Monitors Monitors
		 {
			 set
			 {
				  this._monitors = value;
			 }
		 }

		 public virtual GraphDatabaseFacadeFactory.Dependencies DatabaseDependencies()
		 {
			  return newDependencies().monitors(_monitors).userLogProvider(_userLogProvider).settingsClasses(_settingsClasses).urlAccessRules(_urlAccessRules).kernelExtensions(_kernelExtensions);
		 }
	}

}