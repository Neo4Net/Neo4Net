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
namespace Org.Neo4j.Kernel
{
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;
	using LifecycleStatus = Org.Neo4j.Kernel.Lifecycle.LifecycleStatus;

	public class DummyExtension : Lifecycle
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal LifecycleStatus StatusConflict = LifecycleStatus.NONE;
		 private DummyExtensionFactory.Dependencies _dependencies;

		 public DummyExtension( DummyExtensionFactory.Dependencies dependencies )
		 {
			  this._dependencies = dependencies;
		 }

		 public override void Init()
		 {
			  if ( StatusConflict != LifecycleStatus.NONE )
			  {
					throw new System.InvalidOperationException( "Wrong state:" + StatusConflict );
			  }

			  StatusConflict = LifecycleStatus.STOPPED;
		 }

		 public override void Start()
		 {
			  if ( StatusConflict != LifecycleStatus.STOPPED )
			  {
					throw new System.InvalidOperationException( "Wrong state:" + StatusConflict );
			  }

			  StatusConflict = LifecycleStatus.STARTED;
		 }

		 public override void Stop()
		 {
			  if ( StatusConflict != LifecycleStatus.STARTED )
			  {
					throw new System.InvalidOperationException( "Wrong state:" + StatusConflict );
			  }

			  StatusConflict = LifecycleStatus.STOPPED;
		 }

		 public override void Shutdown()
		 {
			  if ( StatusConflict != LifecycleStatus.STOPPED )
			  {
					throw new System.InvalidOperationException( "Wrong state:" + StatusConflict );
			  }

			  StatusConflict = LifecycleStatus.SHUTDOWN;
		 }

		 public virtual LifecycleStatus Status
		 {
			 get
			 {
				  return StatusConflict;
			 }
		 }

		 public virtual DummyExtensionFactory.Dependencies Dependencies
		 {
			 get
			 {
				  return _dependencies;
			 }
		 }
	}

}