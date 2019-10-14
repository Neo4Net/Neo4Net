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
namespace Neo4Net.Kernel
{
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using LifecycleStatus = Neo4Net.Kernel.Lifecycle.LifecycleStatus;

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