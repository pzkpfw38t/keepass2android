/*
This file is part of Keepass2Android, Copyright 2013 Philipp Crocoll. This file is based on Keepassdroid, Copyright Brian Pellin.

  Keepass2Android is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 2 of the License, or
  (at your option) any later version.

  Keepass2Android is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with Keepass2Android.  If not, see <http://www.gnu.org/licenses/>.
  */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace keepass2android
{
	
	public class SetPasswordDialog : CancelDialog 
	{
		
		internal String mKeyfile;
		private FileOnFinish mFinish;
		
		public SetPasswordDialog(Context context):base(context) {
		}
		
		public SetPasswordDialog(Context context, FileOnFinish finish):base(context) {
			
			mFinish = finish;
		}

		
		public String keyfile() {
			return mKeyfile;
		}
		
		protected override void OnCreate(Bundle savedInstanceState) 
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.set_password);
			
			SetTitle(Resource.String.password_title);
			
			// Ok button
			Button okButton = (Button) FindViewById(Resource.Id.ok);
			okButton.Click += (object sender, EventArgs e) => 
			{
				TextView passView = (TextView) FindViewById(Resource.Id.pass_password);
				String pass = passView.Text;
				TextView passConfView = (TextView) FindViewById(Resource.Id.pass_conf_password);
				String confpass = passConfView.Text;
				
				// Verify that passwords match
				if ( ! pass.Equals(confpass) ) {
					// Passwords do not match
					Toast.MakeText(Context, Resource.String.error_pass_match, ToastLength.Long).Show();
					return;
				}
				
				TextView keyfileView = (TextView) FindViewById(Resource.Id.pass_keyfile);
				String keyfile = keyfileView.Text;
				mKeyfile = keyfile;
				
				// Verify that a password or keyfile is set
				if ( pass.Length == 0 && keyfile.Length == 0 ) {
					Toast.MakeText(Context, Resource.String.error_nopass, ToastLength.Long).Show();
					return;
					
				}
				
				SetPassword sp = new SetPassword(Context, App.getDB(), pass, keyfile, new AfterSave(this, mFinish, new Handler()));
				ProgressTask pt = new ProgressTask(Context, sp, Resource.String.saving_database);
				pt.run();
			};
				

			
			// Cancel button
			Button cancelButton = (Button) FindViewById(Resource.Id.cancel);
			cancelButton.Click += (sender,e) => {
				Cancel();
				if ( mFinish != null ) {
					mFinish.run();
				}
			}; 
		}


		
		class AfterSave : OnFinish {
			private FileOnFinish mFinish;

			SetPasswordDialog dlg;
			
			public AfterSave(SetPasswordDialog dlg, FileOnFinish finish, Handler handler): base(finish, handler) {
				mFinish = finish;
				this.dlg = dlg;
			}
			
			
			public override void run() {
				if ( mSuccess ) {
					if ( mFinish != null ) {
						mFinish.setFilename(dlg.mKeyfile);
					}

					dlg.Dismiss();
				} else {
					displayMessage(dlg.Context);
				}
				
				base.run();
			}
			
		}
		
	}

}
