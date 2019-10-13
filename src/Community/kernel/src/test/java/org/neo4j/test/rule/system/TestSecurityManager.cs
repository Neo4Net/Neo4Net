using System;
using System.Threading;

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
namespace Neo4Net.Test.rule.system
{

	public class TestSecurityManager : SecurityManager
	{

		 private SecurityManager _securityManager;

		 internal TestSecurityManager( SecurityManager securityManager )
		 {
			  this._securityManager = securityManager;
		 }

		 public override void CheckExit( int status )
		 {
			  throw new SystemExitError( status );
		 }

		 public override object SecurityContext
		 {
			 get
			 {
				  return ManagerExists() ? _securityManager.SecurityContext : base.SecurityContext;
			 }
		 }

		 public override void CheckPermission( Permission perm )
		 {
			  // if original security manager exists delegate permission check to it
			  // otherwise silently allow everything
			  if ( ManagerExists() )
			  {
					_securityManager.checkPermission( perm );
			  }
		 }

		 public override void CheckPermission( Permission perm, object context )
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkPermission( perm, context );
			  }
			  else
			  {
					base.CheckPermission( perm, context );
			  }
		 }

		 public override void CheckCreateClassLoader()
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkCreateClassLoader();
			  }
			  else
			  {
					base.CheckCreateClassLoader();
			  }
		 }

		 public override void CheckAccess( Thread t )
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkAccess( t );
			  }
			  else
			  {
					base.CheckAccess( t );
			  }
		 }

		 public override void CheckAccess( ThreadGroup g )
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkAccess( g );
			  }
			  else
			  {
					base.CheckAccess( g );
			  }
		 }

		 public override void CheckExec( string cmd )
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkExec( cmd );
			  }
			  else
			  {
					base.CheckExec( cmd );
			  }
		 }

		 public override void CheckLink( string lib )
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkLink( lib );
			  }
			  else
			  {
					base.CheckLink( lib );
			  }
		 }

		 public override void CheckRead( FileDescriptor fd )
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkRead( fd );
			  }
			  else
			  {
					base.CheckRead( fd );
			  }
		 }

		 public override void CheckRead( string file )
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkRead( file );
			  }
			  else
			  {
					base.CheckRead( file );
			  }
		 }

		 public override void CheckRead( string file, object context )
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkRead( file, context );
			  }
			  else
			  {
					base.CheckRead( file, context );
			  }
		 }

		 public override void CheckWrite( FileDescriptor fd )
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkWrite( fd );
			  }
			  else
			  {
					base.CheckWrite( fd );
			  }
		 }

		 public override void CheckWrite( string file )
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkWrite( file );
			  }
			  else
			  {
					base.CheckWrite( file );
			  }
		 }

		 public override void CheckDelete( string file )
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkDelete( file );
			  }
			  else
			  {
					base.CheckDelete( file );
			  }
		 }

		 public override void CheckConnect( string host, int port )
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkConnect( host, port );
			  }
			  else
			  {
					base.CheckConnect( host, port );
			  }
		 }

		 public override void CheckConnect( string host, int port, object context )
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkConnect( host, port, context );
			  }
			  else
			  {
					base.CheckConnect( host, port, context );
			  }
		 }

		 public override void CheckListen( int port )
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkListen( port );
			  }
			  else
			  {
					base.CheckListen( port );
			  }
		 }

		 public override void CheckAccept( string host, int port )
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkAccept( host, port );
			  }
			  else
			  {
					base.CheckAccept( host, port );
			  }
		 }

		 public override void CheckMulticast( InetAddress maddr )
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkMulticast( maddr );
			  }
			  else
			  {
					base.CheckMulticast( maddr );
			  }
		 }

		 public override void CheckMulticast( InetAddress maddr, sbyte ttl )
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkMulticast( maddr, ttl );
			  }
			  else
			  {
					base.CheckMulticast( maddr, ttl );
			  }
		 }

		 public override void CheckPropertiesAccess()
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkPropertiesAccess();
			  }
			  else
			  {
					base.CheckPropertiesAccess();
			  }
		 }

		 public override void CheckPropertyAccess( string key )
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkPropertyAccess( key );
			  }
			  else
			  {
					base.CheckPropertyAccess( key );
			  }
		 }

		 public override bool CheckTopLevelWindow( object window )
		 {
			  return ManagerExists() ? _securityManager.checkTopLevelWindow(window) : base.CheckTopLevelWindow(window);
		 }

		 public override void CheckPrintJobAccess()
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkPrintJobAccess();
			  }
			  else
			  {
					base.CheckPrintJobAccess();
			  }
		 }

		 public override void CheckSystemClipboardAccess()
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkSystemClipboardAccess();
			  }
			  else
			  {
					base.CheckSystemClipboardAccess();
			  }
		 }

		 public override void CheckAwtEventQueueAccess()
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkAwtEventQueueAccess();
			  }
			  else
			  {
					base.CheckAwtEventQueueAccess();
			  }
		 }

		 public override void CheckPackageAccess( string pkg )
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkPackageAccess( pkg );
			  }
			  else
			  {
					base.CheckPackageAccess( pkg );
			  }
		 }

		 public override void CheckPackageDefinition( string pkg )
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkPackageDefinition( pkg );
			  }
			  else
			  {
					base.CheckPackageDefinition( pkg );
			  }
		 }

		 public override void CheckSetFactory()
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkSetFactory();
			  }
			  else
			  {
					base.CheckSetFactory();
			  }
		 }

		 public override void CheckMemberAccess( Type clazz, int which )
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkMemberAccess( clazz, which );
			  }
			  else
			  {
					base.CheckMemberAccess( clazz, which );
			  }
		 }

		 public override void CheckSecurityAccess( string target )
		 {
			  if ( ManagerExists() )
			  {
					_securityManager.checkSecurityAccess( target );
			  }
			  else
			  {
					base.CheckSecurityAccess( target );
			  }
		 }

		 public override ThreadGroup ThreadGroup
		 {
			 get
			 {
				  return ManagerExists() ? _securityManager.ThreadGroup : base.ThreadGroup;
			 }
		 }

		 private bool ManagerExists()
		 {
			  return _securityManager != null;
		 }

	}

}