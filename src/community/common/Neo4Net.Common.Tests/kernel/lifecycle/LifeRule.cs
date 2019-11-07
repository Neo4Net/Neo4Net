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

namespace Neo4Net.Kernel.Lifecycle
{
   using Description = org.junit.runner.Description;
   using Statement = org.junit.runners.model.Statement;
   using TestRule = org.junit.rules.TestRule;

   /// <summary>
   ///  JUnit rule that allows you to manage lifecycle of a set of instances. Register instances
   ///  and then use the init/start/stop/shutdown methods.
   /// </summary>
   public class LifeRule : TestRule
   {
      private LifeSupport _life = new LifeSupport();
      private readonly bool _autoStart;

      public LifeRule() : this(false)
      {
      }

      public LifeRule(bool autoStart)
      {
         this._autoStart = autoStart;
      }

      //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
      //ORIGINAL LINE: public org.junit.runners.model.Statement apply(final org.junit.runners.model.Statement super, org.junit.runner.Description description)
      public override Statement Apply(Statement @base, Description description)
      {
         return new StatementAnonymousInnerClass(this, @base);
      }

      private class StatementAnonymousInnerClass : Statement
      {
         private readonly LifeRule _outerInstance;

         private Statement @base;

         public StatementAnonymousInnerClass(LifeRule outerInstance, Statement @base)
         {
            this.outerInstance = outerInstance;
            this.@base = @base;
         }

         //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
         //ORIGINAL LINE: public void Evaluate() throws Throwable
         public override void Evaluate()
         {
            try
            {
               if (_outerInstance.autoStart)
               {
                  outerInstance.Start();
               }
               @base.Evaluate();
               _outerInstance.life.shutdown();
            }
            catch (Exception failure)
            {
               try
               {
                  _outerInstance.life.shutdown();
               }
               catch (Exception suppressed)
               {
                  failure.addSuppressed(suppressed);
               }
               throw failure;
            }
            finally
            {
               _outerInstance.life = new LifeSupport();
            }
         }
      }

      public virtual T Add<T>(T instance) where T : ILifecycle
      {
         return _life.add(instance);
      }

      public virtual void Init()
      {
         _life.init();
      }

      public virtual void Start()
      {
         _life.start();
      }

      public virtual void Stop()
      {
         _life.stop();
      }

      public virtual void Shutdown()
      {
         _life.shutdown();
      }
   }
}