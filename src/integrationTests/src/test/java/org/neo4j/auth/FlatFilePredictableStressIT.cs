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
namespace Neo4Net.auth
{
	using Rule = org.junit.Rule;


	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using PredefinedRoles = Neo4Net.Server.security.enterprise.auth.plugin.api.PredefinedRoles;
	using UTF8 = Neo4Net.Strings.UTF8;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

	public class FlatFilePredictableStressIT : FlatFileStressBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.fs.EphemeralFileSystemRule fsRule = new org.Neo4Net.test.rule.fs.EphemeralFileSystemRule();
		 public EphemeralFileSystemRule FsRule = new EphemeralFileSystemRule();

		 internal override FileSystemAbstraction FileSystem
		 {
			 get
			 {
				  return FsRule.get();
			 }
		 }

		 internal override ExecutorService SetupWorkload( int n )
		 {
			  ExecutorService service = Executors.newFixedThreadPool( 2 * n );
			  for ( int i = 0; i < n; i++ )
			  {
					service.submit( new IrrationalUserAdmin( this, i ) );
					service.submit( new IrrationalRoleAdmin( this, i ) );
			  }
			  return service;
		 }

		 private class IrrationalUserAdmin : IrrationalAdmin
		 {
			 private readonly FlatFilePredictableStressIT _outerInstance;

			  internal readonly string Username;
			  internal string Password;
			  internal bool Exists;

			  internal IrrationalUserAdmin( FlatFilePredictableStressIT outerInstance, int number ) : base( outerInstance )
			  {
				  this._outerInstance = outerInstance;
					Username = "user" + number;
					Password = DeviousPassword();
					Exists = false;
					setActions( this.createUser, this.deleteUser, this.changePassword, this.suspend, this.activate, this.assignRole, this.unAssignRole );
			  }

			  public override string ToString()
			  {
					return "IrrationalUserAdmin " + Username;
			  }

			  // __________ ACTIONS ___________

			  internal virtual void CreateUser()
			  {
					try
					{
						 outerInstance.FlatFileRealm.newUser( Username, UTF8.encode( Password ), false );
						 Exists = true;
					}
					catch ( IOException e )
					{
						 outerInstance.Errors.Add( e );
					}
					catch ( InvalidArgumentsException e )
					{
						 if ( !Exists || !e.Message.contains( "The specified user '" + Username + "' already exists" ) )
						 {
							  outerInstance.Errors.Add( e );
						 }
					}
			  }

			  internal virtual void DeleteUser()
			  {
					try
					{
						 outerInstance.FlatFileRealm.deleteUser( Username );
						 Exists = false;
					}
					catch ( IOException e )
					{
						 outerInstance.Errors.Add( e );
					}
					catch ( InvalidArgumentsException e )
					{
						 if ( !ValidUserDoesNotExist( e ) )
						 {
							  outerInstance.Errors.Add( e );
						 }
					}
			  }

			  internal virtual void ChangePassword()
			  {
					string newPassword = DeviousPassword();
					try
					{
						 outerInstance.FlatFileRealm.setUserPassword( Username, UTF8.encode( newPassword ), false );
						 Password = newPassword;
					}
					catch ( IOException e )
					{
						 outerInstance.Errors.Add( e );
					}
					catch ( InvalidArgumentsException e )
					{
						 if ( !ValidUserDoesNotExist( e ) && !ValidSamePassword( newPassword, e ) )
						 {
							  outerInstance.Errors.Add( e );
						 }
					}
			  }

			  internal virtual void Suspend()
			  {
					try
					{
						 outerInstance.FlatFileRealm.suspendUser( Username );
					}
					catch ( IOException e )
					{
						 outerInstance.Errors.Add( e );
					}
					catch ( InvalidArgumentsException e )
					{
						 if ( !ValidUserDoesNotExist( e ) )
						 {
							  outerInstance.Errors.Add( e );
						 }
					}
			  }

			  internal virtual void Activate()
			  {
					try
					{
						 outerInstance.FlatFileRealm.activateUser( Username, false );
					}
					catch ( IOException e )
					{
						 outerInstance.Errors.Add( e );
					}
					catch ( InvalidArgumentsException e )
					{
						 if ( !ValidUserDoesNotExist( e ) )
						 {
							  outerInstance.Errors.Add( e );
						 }
					}
			  }

			  internal virtual void AssignRole()
			  {
					string role = RandomRole();
					try
					{
						 outerInstance.FlatFileRealm.addRoleToUser( role, Username );
					}
					catch ( IOException e )
					{
						 outerInstance.Errors.Add( e );
					}
					catch ( InvalidArgumentsException e )
					{
						 if ( !ValidUserDoesNotExist( e ) )
						 {
							  outerInstance.Errors.Add( e );
						 }
					}
			  }

			  internal virtual void UnAssignRole()
			  {
					string role = RandomRole();
					try
					{
						 outerInstance.FlatFileRealm.removeRoleFromUser( role, Username );
					}
					catch ( IOException e )
					{
						 outerInstance.Errors.Add( e );
					}
					catch ( InvalidArgumentsException e )
					{
						 if ( !ValidUserDoesNotExist( e ) )
						 {
							  outerInstance.Errors.Add( e );
						 }
					}
			  }

			  // ______________ HELPERS ______________

			  internal virtual string DeviousPassword()
			  {
					return Random.nextBoolean() ? "123" : "321";
			  }

			  internal readonly string[] PredefinedRoles = new string[] { PredefinedRoles.READER, PredefinedRoles.PUBLISHER, PredefinedRoles.ARCHITECT, PredefinedRoles.ADMIN };

			  internal virtual string RandomRole()
			  {
					return PredefinedRoles[Random.Next( PredefinedRoles.Length )];
			  }

			  internal virtual bool ValidSamePassword( string newPassword, InvalidArgumentsException e )
			  {
					return newPassword.Equals( Password ) && e.Message.contains( "Old password and new password cannot be the same." );
			  }

			  internal virtual bool ValidUserDoesNotExist( InvalidArgumentsException e )
			  {
					return !Exists && e.Message.contains( "User '" + Username + "' does not exist" );
			  }
		 }

		 private class IrrationalRoleAdmin : IrrationalAdmin
		 {
			 private readonly FlatFilePredictableStressIT _outerInstance;

			  internal readonly string Username;
			  internal readonly string RoleName;
			  internal bool Exists;

			  internal IrrationalRoleAdmin( FlatFilePredictableStressIT outerInstance, int number ) : base( outerInstance )
			  {
				  this._outerInstance = outerInstance;
					Username = "user" + number;
					RoleName = "role" + number;
					Exists = false;
					setActions( this.createRole, this.deleteRole, this.assignRole, this.unAssignRole );
			  }

			  public override string ToString()
			  {
					return "IrrationalRoleAdmin " + RoleName;
			  }

			  // __________ ACTIONS ___________

			  internal virtual void CreateRole()
			  {
					try
					{
						 outerInstance.FlatFileRealm.newRole( RoleName, Username );
						 Exists = true;
					}
					catch ( IOException e )
					{
						 outerInstance.Errors.Add( e );
					}
					catch ( InvalidArgumentsException e )
					{
						 if ( !ValidRoleExists( e ) && !UserDoesNotExist( e ) )
						 {
							  outerInstance.Errors.Add( e );
						 }
					}
			  }

			  internal virtual void DeleteRole()
			  {
					try
					{
						 outerInstance.FlatFileRealm.deleteRole( RoleName );
						 Exists = false;
					}
					catch ( IOException e )
					{
						 outerInstance.Errors.Add( e );
					}
					catch ( InvalidArgumentsException e )
					{
						 if ( !ValidRoleDoesNotExist( e ) )
						 {
							  outerInstance.Errors.Add( e );
						 }
					}
			  }

			  internal virtual void AssignRole()
			  {
					try
					{
						 outerInstance.FlatFileRealm.addRoleToUser( RoleName, "Neo4Net" );
					}
					catch ( IOException e )
					{
						 outerInstance.Errors.Add( e );
					}
					catch ( InvalidArgumentsException e )
					{
						 if ( !ValidRoleDoesNotExist( e ) )
						 {
							  outerInstance.Errors.Add( e );
						 }
					}
			  }

			  internal virtual void UnAssignRole()
			  {
					try
					{
						 outerInstance.FlatFileRealm.removeRoleFromUser( RoleName, "Neo4Net" );
					}
					catch ( IOException e )
					{
						 outerInstance.Errors.Add( e );
					}
					catch ( InvalidArgumentsException e )
					{
						 if ( !ValidRoleDoesNotExist( e ) )
						 {
							  outerInstance.Errors.Add( e );
						 }
					}
			  }

			  // ______________ HELPERS ______________

			  internal virtual bool ValidRoleExists( InvalidArgumentsException e )
			  {
					return Exists && e.Message.contains( "The specified role '" + RoleName + "' already exists" );
			  }

			  internal virtual bool ValidRoleDoesNotExist( InvalidArgumentsException e )
			  {
					return !Exists && e.Message.contains( "Role '" + RoleName + "' does not exist" );
			  }

			  internal virtual bool UserDoesNotExist( InvalidArgumentsException e )
			  {
					return e.Message.contains( "User '" + Username + "' does not exist" );
			  }
		 }
	}

}